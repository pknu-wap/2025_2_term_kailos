using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    public enum TimelineMode
    {
        StartTimes,   // timeline[i] = '시작 시각(초)'; 창=다음 타점과의 간격(꼬리는 defaultTail)
        Windows       // timeline[i] = '창 길이(초)'; 그 시간 안에 페이드 인/아웃 완료
    }

    [SerializeField] private AttackAnimationController anim;

    [Header("Manual Centers (World Positions)")]
    [SerializeField] private Vector3[] playerCentersPos = new Vector3[16];
    [SerializeField] private Vector3[] enemyCentersPos = new Vector3[16];

    [Header("Hit AABB (for damage check)")]
    [SerializeField] private float tileWidth = 1.3f;
    [SerializeField] private float tileHeight = 1.3f;
    [SerializeField] private float tileZ = 0f;

    [Header("Timeline Interpretation")]
    [SerializeField] private TimelineMode timelineMode = TimelineMode.Windows; // ★ 기본: 창 길이로 해석
    [SerializeField] private float defaultTailWindow = 0.20f;  // StartTimes 모드 꼬리시간
    [SerializeField] private float minWindow = 0.06f;  // 양 모드 공통 보정

    [Header("Hit/VFX")]
    [SerializeField] private HPController hp;
    [SerializeField] private bool oneHitPerCard = true;

    void Awake()
    {
        if (!hp) hp = FindObjectOfType<HPController>(true);
        if (!anim) anim = FindObjectOfType<AttackAnimationController>(true);
    }

    public IEnumerator Execute(AttackCardSO so, Faction self, Faction foe)
    {
        if (!so) yield break;
        if (!anim) { Debug.LogWarning("[AttackController] AttackAnimationController missing."); yield break; }

        hp?.BeginCardHitTest(foe);

        var centers = (foe == Faction.Player) ? playerCentersPos : enemyCentersPos;
        if (centers == null || centers.Length < 16)
        { Debug.LogWarning("[AttackController] centers pos array must have 16 elements."); yield break; }

        anim.EnsurePool(16);

        if (so.waves == null || so.waves.Length == 0)
        {
            bool[] mask = new bool[16];
            float[] times = new float[16];
            AttackCardSO.ParsePattern16(so.pattern16, mask);
            AttackCardSO.FillTimeline16(so.timeline, times);

            if (IsAllZero(mask)) yield break;

            anim.PlaceAndShowMask(mask, centers);
            yield return RunTimeline(mask, times, centers, so, self, foe);
            anim.HideAll();
            yield break;
        }

        for (int wi = 0; wi < so.waves.Length; wi++)
        {
            var w = so.waves[wi];
            if (w == null) continue;

            if (w.delayBefore > 0f) yield return new WaitForSeconds(w.delayBefore);

            bool[] mask = new bool[16];
            float[] times = new float[16];
            AttackCardSO.ParsePattern16(w.pattern16, mask);
            AttackCardSO.FillTimeline16(w.timeline, times);

            if (IsAllZero(mask))
            {
                if (w.delayAfter > 0f) yield return new WaitForSeconds(w.delayAfter);
                continue;
            }

            anim.PlaceAndShowMask(mask, centers);

            // (선택) 웨이브 시작 SFX/VFX
            if (!w.sfxEveryHit && w.sfx)
                AudioSource.PlayClipAtPoint(w.sfx, AveragePosition(mask, centers));
            if (!w.vfxEveryHit && w.vfxPrefab)
                SpawnVfx(w.vfxPrefab, AveragePosition(mask, centers), w.vfxLifetime);

            yield return RunTimeline(mask, times, centers, so, self, foe);

            anim.HideAll();

            if (w.delayAfter > 0f) yield return new WaitForSeconds(w.delayAfter);
        }
    }

    private IEnumerator RunTimeline(
        bool[] mask, float[] timeline, Vector3[] centers,
        AttackCardSO so, Faction self, Faction foe)
    {
        var schedule = BuildSchedule(mask, timeline);
        bool damageApplied = false;

        // ★ 피크 콜백: 피크에 도달한 타일 인덱스로 판정 & 데미지
        System.Action<int> onPeak = (tileIdx) =>
        {
            if (tileIdx < 0 || tileIdx >= centers.Length) return;
            if (damageApplied && oneHitPerCard) return;

            Vector3 hitPos = centers[tileIdx]; hitPos.z = tileZ;
            if (CheckHitNow(hitPos))
                ApplyDamageOnce(so, self, foe, ref damageApplied);
        };

        // ★ 구독
        anim.OnTilePeak += onPeak;

        // 코루틴이 어떤 경로로 끝나든 해제되도록 try/finally 사용
        try
        {
            if (timelineMode == TimelineMode.StartTimes)
            {
                // 시각에 맞춰 대기 → 창=다음 타점 간격(없으면 defaultTailWindow)
                float lastT = 0f;
                for (int k = 0; k < schedule.Count; k++)
                {
                    var (idx, tPoint) = schedule[k];

                    float wait = Mathf.Max(0f, tPoint - lastT);
                    if (wait > 0f) yield return new WaitForSeconds(wait);
                    lastT = tPoint;

                    float nextT = (k + 1 < schedule.Count) ? schedule[k + 1].time : tPoint + defaultTailWindow;
                    float window = Mathf.Max(minWindow, nextT - tPoint);

                    // 창 시작 → 피크 순간에 onPeak 호출됨
                    anim.StartPulseWindow(idx, window);
                }

                // (선택) 약간의 꼬리 대기
                if (schedule.Count > 0 && defaultTailWindow > 0f)
                    yield return new WaitForSeconds(defaultTailWindow);
            }
            else // TimelineMode.Windows
            {
                // 각 칸의 timeline[i]가 곧 "페이드 총 길이". 0 또는 음수면 무시.
                float maxWin = 0f;

                for (int i = 0; i < 16; i++)
                {
                    if (mask != null && i < mask.Length && mask[i])
                    {
                        float win = (timeline != null && timeline.Length > i) ? timeline[i] : 0f;
                        if (win > 0f)
                        {
                            anim.StartPulseWindow(i, win);  // 피크 시 onPeak 호출
                            if (win > maxWin) maxWin = win;
                        }
                    }
                }

                if (maxWin > 0f) yield return new WaitForSeconds(maxWin);
            }
        }
        finally
        {
            // ★ 꼭 해제 (중복 타격/메모리 릭 방지)
            anim.OnTilePeak -= onPeak;
        }

        yield break;
    }

    private List<(int idx, float time)> BuildSchedule(bool[] mask, float[] times)
    {
        var list = new List<(int idx, float time)>(16);
        for (int i = 0; i < 16; i++)
        {
            if (mask != null && i < mask.Length && mask[i])
            {
                float t = (times != null && times.Length > i) ? Mathf.Max(0f, times[i]) : 0f;
                list.Add((i, t));
            }
        }
        list.Sort((a, b) => a.time.CompareTo(b.time));
        return list;
    }

    private bool CheckHitNow(Vector3 center)
    {
        var foePos = hp ? hp.GetWorldPositionOfPawn(hp.CurrentDamageTarget) : Vector3.positiveInfinity;
        float halfX = tileWidth * 0.5f, halfY = tileHeight * 0.5f;
        return (foePos.x >= center.x - halfX && foePos.x <= center.x + halfX &&
                foePos.y >= center.y - halfY && foePos.y <= center.y + halfY);
    }

    private void ApplyDamageOnce(AttackCardSO so, Faction self, Faction foe, ref bool damageAppliedFlag)
    {
        if (damageAppliedFlag && oneHitPerCard) return;
        if (hp == null) { Debug.LogWarning("[AttackController] HPController missing."); return; }

        int atk = hp.GetAttack(self);
        int def = hp.GetDefense(foe);
        int dmg = Mathf.Max(1, (so.power + atk) - def);
        hp.ApplyDamage(foe, dmg);
        damageAppliedFlag = true;
    }

    private static bool IsAllZero(bool[] mask)
    {
        if (mask == null || mask.Length == 0) return true;
        for (int i = 0; i < mask.Length; i++) if (mask[i]) return false;
        return true;
    }

    private static Vector3 AveragePosition(bool[] mask, Vector3[] centers)
    {
        Vector3 sum = Vector3.zero; int cnt = 0;
        for (int i = 0; i < 16 && i < centers.Length; i++)
            if (mask != null && i < mask.Length && mask[i]) { var p = centers[i]; p.z = 0f; sum += p; cnt++; }
        if (cnt == 0) return Vector3.zero;
        var avg = sum / cnt; avg.z = 0f; return avg;
    }

    private static void SpawnVfx(GameObject prefab, Vector3 pos, float life)
    {
        if (!prefab) return;
        var go = GameObject.Instantiate(prefab, pos, Quaternion.identity);
        if (life > 0f) GameObject.Destroy(go, life);
    }
}
