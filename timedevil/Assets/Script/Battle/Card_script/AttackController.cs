using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [Header("Visual Only Scale")]
    [SerializeField, Range(0.2f, 1.5f)] private float visualScale = 0.8f;

    [Header("Manual Centers (World Positions)")]
    [SerializeField] private Vector3[] playerCentersPos = new Vector3[16];
    [SerializeField] private Vector3[] enemyCentersPos = new Vector3[16];

    [Header("Tile Visual")]
    [SerializeField] private string resourcesFolder = "my_asset";
    [SerializeField] private string spriteName = "attack"; // Resources/my_asset/attack.png
    [SerializeField] private float tileWidth = 1.3f;
    [SerializeField] private float tileHeight = 1.3f;
    [SerializeField] private string sortingLayer = "Default";
    [SerializeField] private int sortingOrder = 50;
    [SerializeField] private float tileZ = 0f;

    [Header("Fade Pulse")]
    [SerializeField, Range(0f, 1f)] private float peakAlpha = 0.75f; // 최댓값
    [SerializeField] private float minWindow = 0.06f;                // 너무 짧을 때 최소창
    [SerializeField] private float defaultTailWindow = 0.20f;        // 마지막 타점 꼬리시간

    [Header("Hit/VFX")]
    [SerializeField] private HPController hp;
    [SerializeField, Tooltip("같은 카드 수행 중에는 한 번만 맞게 함")]
    private bool oneHitPerCard = true;

    private Sprite _sprite;
    private readonly List<GameObject> _pool = new List<GameObject>();
    private Coroutine[] _pulses; // 타일별 펄스 코루틴

    void Awake()
    {
        if (hp == null) hp = FindObjectOfType<HPController>(true);
        _sprite = Resources.Load<Sprite>($"{resourcesFolder}/{spriteName}");
        if (_sprite == null)
            Debug.LogWarning($"[AttackController] Sprite not found at Resources/{resourcesFolder}/{spriteName}.png");
    }

    public IEnumerator Execute(AttackCardSO so, Faction self, Faction foe)
    {
        if (!so) yield break;

        hp?.BeginCardHitTest(foe);

        var centers = (foe == Faction.Player) ? playerCentersPos : enemyCentersPos;
        if (centers == null || centers.Length < 16)
        {
            Debug.LogWarning("[AttackController] centers pos array must have 16 elements.");
            yield break;
        }

        EnsurePoolSize(16);

        // waves가 없으면 레거시 1회
        if (so.waves == null || so.waves.Length == 0)
        {
            bool[] mask = new bool[16];
            float[] times = new float[16];
            AttackCardSO.ParsePattern16(so.pattern16, mask);
            AttackCardSO.FillTimeline16(so.timeline, times);

            if (IsAllZero(mask)) yield break;

            ShowTiles(mask, centers);
            yield return RunTimeline(mask, times, centers, so, self, foe);
            HideAllTiles();
            yield break;
        }

        // 웨이브 순차
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

            ShowTiles(mask, centers);
            yield return RunTimeline(mask, times, centers, so, self, foe);
            HideAllTiles();

            if (w.delayAfter > 0f) yield return new WaitForSeconds(w.delayAfter);
        }
    }

    // =============== 내부 유틸 ===============

    private IEnumerator RunTimeline(
        bool[] mask, float[] times, Vector3[] centers,
        AttackCardSO so,
        Faction self, Faction foe)
    {
        var schedule = BuildSchedule(mask, times);
        bool damageApplied = false;

        for (int k = 0; k < schedule.Count; k++)
        {
            var (idx, tPoint) = schedule[k];

            // 현재 타점까지 대기
            float prevT = (k == 0) ? 0f : schedule[k - 1].time;
            float wait = Mathf.Max(0f, tPoint - prevT);
            if (wait > 0f) yield return new WaitForSeconds(wait);

            if (idx < 0 || idx >= centers.Length) continue;

            // 다음 타점과의 간격 = 윈도우
            float nextT = (k + 1 < schedule.Count) ? schedule[k + 1].time
                                                   : tPoint + defaultTailWindow;
            float window = Mathf.Max(minWindow, nextT - tPoint);

            // 타일 펄스(창 절반씩 인/아웃)
            StartPulse(idx, window);

            // 히트 판정 + 데미지
            if (!damageApplied || !oneHitPerCard)
            {
                Vector3 hitPos = centers[idx]; hitPos.z = tileZ;
                if (CheckHitNow(hitPos))
                {
                    ApplyDamageOnce(so, self, foe, ref damageApplied);
                }
            }
        }
        // 이후 남은 펄스는 각자 코루틴에서 자연소멸
        yield break;
    }

    private List<(int idx, float time)> BuildSchedule(bool[] mask, float[] times)
    {
        var list = new List<(int idx, float time)>(16);
        for (int i = 0; i < 16; i++)
        {
            if (mask[i])
            {
                float t = (times != null && times.Length > i) ? Mathf.Max(0f, times[i]) : 0f;
                list.Add((i, t));
            }
        }
        list.Sort((a, b) => a.time.CompareTo(b.time));
        return list;
    }

    private void ShowTiles(bool[] mask, Vector3[] centers)
    {
        EnsurePoolSize(16);
        for (int i = 0; i < 16; i++)
        {
            var go = _pool[i];
            if (!go) continue;

            bool on = (mask != null && i < mask.Length && mask[i] && centers != null && i < centers.Length);
            if (on)
            {
                var pos = centers[i]; pos.z = tileZ;
                go.transform.position = pos;

                var sr = go.GetComponent<SpriteRenderer>();
                if (sr)
                {
                    // 시작은 투명(펄스 때만 보임)
                    var c = sr.color;
                    c.r = 1f; c.g = 0f; c.b = 0f; c.a = 0f;
                    sr.color = c;

                    go.transform.localScale = ComputeSpriteScale(sr);
                    sr.sortingLayerName = sortingLayer;
                    sr.sortingOrder = sortingOrder;
                }
                go.SetActive(true);
            }
            else
            {
                StopPulseIfAny(i);
                if (go) go.SetActive(false);
            }
        }
    }

    private void HideAllTiles()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            StopPulseIfAny(i);
            var go = _pool[i];
            if (go) go.SetActive(false);
        }
    }

    private void EnsurePoolSize(int need)
    {
        if (_sprite == null) return;
        while (_pool.Count < need)
        {
            var go = new GameObject("AttackTile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _sprite;
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = sortingOrder;

            // 풀 생성 시 기본 투명
            var c = sr.color; c.r = 1f; c.g = 0f; c.b = 0f; c.a = 0f;
            sr.color = c;

            go.transform.localScale = ComputeSpriteScale(sr);
            go.SetActive(false);
            _pool.Add(go);
        }

        if (_pulses == null || _pulses.Length < _pool.Count)
            _pulses = new Coroutine[_pool.Count];
    }

    private Vector3 ComputeSpriteScale(SpriteRenderer sr)
    {
        if (sr == null || sr.sprite == null) return Vector3.one;
        var s = sr.sprite.bounds.size;
        float sx = (s.x > 0f) ? (tileWidth / s.x) : 1f;
        float sy = (s.y > 0f) ? (tileHeight / s.y) : 1f;
        return new Vector3(sx * visualScale, sy * visualScale, 1f);
    }

    private bool CheckHitNow(Vector3 center)
    {
        var foePos = hp ? hp.GetWorldPositionOfPawn(hp.CurrentDamageTarget) : Vector3.positiveInfinity;
        float halfX = tileWidth * 0.5f;
        float halfY = tileHeight * 0.5f;

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
        {
            if (mask != null && i < mask.Length && mask[i])
            {
                var p = centers[i]; p.z = 0f;
                sum += p; cnt++;
            }
        }
        if (cnt == 0) return Vector3.zero;
        var avg = sum / cnt; avg.z = 0f;
        return avg;
    }

    private static void SpawnVfx(GameObject prefab, Vector3 pos, float life)
    {
        if (!prefab) return;
        var go = GameObject.Instantiate(prefab, pos, Quaternion.identity);
        if (life > 0f) GameObject.Destroy(go, life);
    }

    // ---------- Pulse 구현 ----------

    private void StartPulse(int idx, float windowSeconds)
    {
        if (idx < 0 || idx >= _pool.Count) return;

        StopPulseIfAny(idx);
        float fin = Mathf.Max(0f, windowSeconds * 0.5f);
        float fout = Mathf.Max(0f, windowSeconds * 0.5f);
        _pulses[idx] = StartCoroutine(PulseTileOnce(idx, fin, fout));
    }

    private void StopPulseIfAny(int idx)
    {
        if (_pulses == null) return;
        if (idx < 0 || idx >= _pulses.Length) return;
        if (_pulses[idx] != null)
        {
            StopCoroutine(_pulses[idx]);
            _pulses[idx] = null;
        }
    }

    private IEnumerator PulseTileOnce(int idx, float fadeInSec, float fadeOutSec)
    {
        if (idx < 0 || idx >= _pool.Count) yield break;
        var go = _pool[idx];
        if (!go || !go.activeInHierarchy) { _pulses[idx] = null; yield break; }

        var sr = go.GetComponent<SpriteRenderer>();
        if (!sr) { _pulses[idx] = null; yield break; }

        var c = sr.color;
        float peak = Mathf.Clamp01(peakAlpha);

        // start 0 → peak
        c.a = 0f; sr.color = c;
        if (fadeInSec > 0f)
        {
            float t = 0f;
            while (t < fadeInSec)
            {
                if (!go.activeInHierarchy) { _pulses[idx] = null; yield break; }
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0f, peak, t / fadeInSec);
                sr.color = c;
                yield return null;
            }
        }
        else { c.a = peak; sr.color = c; }

        // peak → 0
        if (fadeOutSec > 0f)
        {
            float t = 0f;
            while (t < fadeOutSec)
            {
                if (!go.activeInHierarchy) { _pulses[idx] = null; yield break; }
                t += Time.deltaTime;
                c.a = Mathf.Lerp(peak, 0f, t / fadeOutSec);
                sr.color = c;
                yield return null;
            }
        }

        c.a = 0f; sr.color = c;
        _pulses[idx] = null;
    }
}
