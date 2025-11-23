using System;
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
    [SerializeField] private TimelineMode timelineMode = TimelineMode.Windows; // 기본: 창 길이 해석
    [SerializeField] private float defaultTailWindow = 0.20f;               // StartTimes 꼬리시간
    [SerializeField] private float minWindow = 0.06f;               // 최소 창 길이 보정

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
        {
            Debug.LogWarning("[AttackController] centers pos array must have 16 elements.");
            yield break;
        }

        anim.EnsurePool(16);

        // ─────────────────────────────
        // 레거시: waves 없음 → pattern/timeline만 사용
        // ─────────────────────────────
        if (so.waves == null || so.waves.Length == 0)
        {
            bool[] mask = new bool[16];
            float[] times = new float[16];
            AttackCardSO.ParsePattern16(so.pattern16, mask);
            AttackCardSO.FillTimeline16(so.timeline, times);

            if (IsAllZero(mask)) yield break;

            anim.PlaceAndShowMask(mask, centers);
            yield return RunTimeline(mask, times, centers, so, self, foe, null);
            anim.HideAll();
            yield break;
        }

        // ─────────────────────────────
        // 웨이브 기반 카드
        // ─────────────────────────────
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

            // ★ 경고(빨간 타일) 표시 + 칸별 히트 지연 스케줄
            yield return RunTimeline(mask, times, centers, so, self, foe, w);

            anim.HideAll();

            if (w.delayAfter > 0f) yield return new WaitForSeconds(w.delayAfter);
        }
    }

    /// <summary>
    /// 경고(타일 페이드)와 실제 히트(칸별 지연)를 분리해서 실행.
    /// waveOrNull==null → 레거시: hitDelay로 timeline 재활용
    /// </summary>
    private IEnumerator RunTimeline(
        bool[] mask, float[] timeline, Vector3[] centers,
        AttackCardSO so, Faction self, Faction foe,
        AttackCardSO.Wave waveOrNull // 웨이브 전달(없으면 null)
    )
    {
        // ── 1) 경고(빨간 타일) 표시 ──
        if (timelineMode == TimelineMode.StartTimes)
        {
            var schedule = BuildSchedule(mask, timeline);
            float lastT = 0f;
            for (int k = 0; k < schedule.Count; k++)
            {
                var (idx, tPoint) = schedule[k];
                float wait = Mathf.Max(0f, tPoint - lastT);
                if (wait > 0f) yield return new WaitForSeconds(wait);
                lastT = tPoint;

                float nextT = (k + 1 < schedule.Count) ? schedule[k + 1].time : tPoint + defaultTailWindow;
                float window = Mathf.Max(minWindow, nextT - tPoint);
                anim.StartPulseWindow(idx, window); // 경고만
            }
            if (schedule.Count > 0 && defaultTailWindow > 0f)
                yield return new WaitForSeconds(defaultTailWindow);
        }
        else // Windows
        {
            float maxWin = 0f;
            for (int i = 0; i < 16; i++)
            {
                if (mask != null && mask[i])
                {
                    float win = (timeline != null && timeline.Length > i) ? timeline[i] : 0f;
                    if (win > 0f)
                    {
                        anim.StartPulseWindow(i, win); // 경고만
                        if (win > maxWin) maxWin = win;
                    }
                }
            }
            if (maxWin > 0f) yield return new WaitForSeconds(maxWin);
        }

        // ── 2) 실제 히트: 칸별 '히트 지연'으로 스케줄 ──
        // 웨이브가 있으면 wave.hitDelays, 없으면 레거시: timeline을 지연으로 사용
        var hitDelays = new float[16];
        for (int i = 0; i < 16; i++)
        {
            float d = 0f;
            if (waveOrNull != null && waveOrNull.hitDelays != null && waveOrNull.hitDelays.Length > i)
                d = Mathf.Max(0f, waveOrNull.hitDelays[i]);
            else if (timeline != null && timeline.Length > i)
                d = Mathf.Max(0f, timeline[i]); // 레거시: 경고와 같은 지연 사용
            hitDelays[i] = d;
        }

        bool damageApplied = false;
        var running = new List<Coroutine>(16);

        for (int i = 0; i < 16; i++)
        {
            if (mask != null && mask[i])
            {
                var center = centers[i]; center.z = tileZ;
                float delay = hitDelays[i];
                running.Add(StartCoroutine(CoHitAfterDelay(
                    delay, center, so, self, foe, waveOrNull,
                    () => damageApplied, () => damageApplied = true
                )));
            }
        }

        // 모든 칸 판정 완료 대기
        foreach (var co in running)
            if (co != null) yield return co;
    }

    /// <summary>칸별 지연 후 VFX 스폰 + 판정 + 데미지(옵션: 카드당 1회)</summary>
    private IEnumerator CoHitAfterDelay(
        float delay, Vector3 center,
        AttackCardSO so, Faction self, Faction foe,
        AttackCardSO.Wave waveOrNull,
        Func<bool> getDamageApplied,
        Action setDamageAppliedTrue
    )
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        // 히트 순간 VFX (웨이브 설정이 있고, 히트마다 스폰하기로 한 경우)
        if (waveOrNull != null && waveOrNull.vfxPrefab && waveOrNull.vfxEveryHit)
            SpawnVfx(waveOrNull.vfxPrefab, center, waveOrNull.vfxLifetime);

        // 카드당 1회 옵션
        if (getDamageApplied() && oneHitPerCard) yield break;

        // 실제 히트 AABB 판정
        if (CheckHitNow(center))
        {
            // ApplyDamageOnce 로직을 여기서 직접 수행 (ref 전달 문제 회피)
            if (hp == null) yield break;

            int atk = hp.GetAttack(self);
            int def = hp.GetDefense(foe);
            int dmg = Mathf.Max(1, (so.power + atk) - def);
            hp.ApplyDamage(foe, dmg);

            setDamageAppliedTrue();
        }
    }

    // ─────────────────────────────
    // 유틸
    // ─────────────────────────────
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
