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
    [SerializeField, Range(0f, 1f)] private float alpha = 0.6f;
    [SerializeField] private float tileWidth = 1.3f;
    [SerializeField] private float tileHeight = 1.3f;
    [SerializeField] private string sortingLayer = "Default";
    [SerializeField] private int sortingOrder = 50;
    [SerializeField] private float tileZ = 0f;

    [Header("Hit/VFX")]
    [SerializeField] private HPController hp;
    [SerializeField, Tooltip("같은 카드 수행 중에는 한 번만 맞게 함")]
    private bool oneHitPerCard = true;

    private Sprite _sprite;
    private readonly List<GameObject> _pool = new List<GameObject>();

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

        // 타겟 팩션 지정(피격 위치 조회용)
        hp?.BeginCardHitTest(foe);

        // 표시 보드(공격자는 상대 보드에 뿌림)
        var centers = (foe == Faction.Player) ? playerCentersPos : enemyCentersPos;
        if (centers == null || centers.Length < 16)
        {
            Debug.LogWarning("[AttackController] centers pos array must have 16 elements.");
            yield break;
        }

        EnsurePoolSize(16);

        // waves가 없으면 레거시 단일 패턴을 웨이브처럼 1회 실행
        if (so.waves == null || so.waves.Length == 0)
        {
            bool[] mask = new bool[16];
            float[] times = new float[16];
            AttackCardSO.ParsePattern16(so.pattern16, mask);
            AttackCardSO.FillTimeline16(so.timeline, times);

            if (IsAllZero(mask)) yield break;

            ShowTiles(mask, centers);
            yield return RunTimeline(mask, times, centers, so,
                waveSfx: null, sfxEveryHit: false,
                vfxPrefab: null, vfxEveryHit: false, vfxLife: 0f,
                self, foe);
            HideAllTiles();
            yield break;
        }

        // 웨이브 순차 실행
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

            // 웨이브 시작 한 번만 SFX/VFX를 원하면 여기서 처리
            if (!w.sfxEveryHit && w.sfx)
                AudioSource.PlayClipAtPoint(w.sfx, AveragePosition(mask, centers));
            if (!w.vfxEveryHit && w.vfxPrefab)
                SpawnVfx(w.vfxPrefab, AveragePosition(mask, centers), w.vfxLifetime);

            // 타임라인 진행(각 타점에서 히트/FX)
            yield return RunTimeline(mask, times, centers, so,
                w.sfx, w.sfxEveryHit,
                w.vfxPrefab, w.vfxEveryHit, w.vfxLifetime,
                self, foe);

            HideAllTiles();

            if (w.delayAfter > 0f) yield return new WaitForSeconds(w.delayAfter);
        }
    }

    // =============== 내부 유틸 ===============

    private IEnumerator RunTimeline(
        bool[] mask, float[] times, Vector3[] centers,
        AttackCardSO so,
        AudioClip waveSfx, bool sfxEveryHit,
        GameObject vfxPrefab, bool vfxEveryHit, float vfxLife,
        Faction self, Faction foe)
    {
        var schedule = BuildSchedule(mask, times);
        bool damageApplied = false;
        float lastT = 0f;

        foreach (var (idx, tPoint) in schedule)
        {
            float wait = Mathf.Max(0f, tPoint - lastT);
            if (wait > 0f) yield return new WaitForSeconds(wait);
            lastT = tPoint;

            if (idx < 0 || idx >= centers.Length) continue;

            // 타점 SFX/VFX (매 히트 모드일 때)
            Vector3 hitPos = centers[idx]; hitPos.z = tileZ;
            if (sfxEveryHit && waveSfx) AudioSource.PlayClipAtPoint(waveSfx, hitPos);
            if (vfxEveryHit && vfxPrefab) SpawnVfx(vfxPrefab, hitPos, vfxLife);

            // 히트 판정 + 데미지
            if (!damageApplied || !oneHitPerCard)
            {
                if (CheckHitNow(hitPos))
                {
                    ApplyDamageOnce(so, self, foe, ref damageApplied);
                }
            }
        }
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
                    sr.color = new Color(1f, 0f, 0f, alpha);
                    go.transform.localScale = ComputeSpriteScale(sr);
                    sr.sortingLayerName = sortingLayer;
                    sr.sortingOrder = sortingOrder;
                }
                go.SetActive(true);
            }
            else go.SetActive(false);
        }
    }

    private void HideAllTiles()
    {
        foreach (var go in _pool) if (go) go.SetActive(false);
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
            sr.color = new Color(1f, 0f, 0f, alpha);
            go.transform.localScale = ComputeSpriteScale(sr);
            go.SetActive(false);
            _pool.Add(go);
        }
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
}
