using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [Header("Visual Only Scale")]
    [SerializeField, Range(0.2f, 1.5f)] private float visualScale = 0.8f; // 그림만 축소/확대

    [Header("Manual Centers (World Positions)")]
    [Tooltip("플레이어 보드 4x4 각 칸 중심의 월드 좌표 (index=r*4+c, r:0..3 상→하, c:0..3 좌→우)")]
    [SerializeField] private Vector3[] playerCentersPos = new Vector3[16];
    [Tooltip("적 보드 4x4 각 칸 중심의 월드 좌표 (index=r*4+c, r:0..3 상→하, c:0..3 좌→우)")]
    [SerializeField] private Vector3[] enemyCentersPos = new Vector3[16];

    [Header("Tile Visual")]
    [SerializeField] private string resourcesFolder = "my_asset";
    [SerializeField] private string spriteName = "attack";     // Resources/my_asset/attack.png
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
        if (_sprite == null) Debug.LogWarning($"[AttackController] Sprite not found at Resources/{resourcesFolder}/{spriteName}.png");
    }

    public IEnumerator Execute(AttackCardSO so, Faction self, Faction foe)
    {
        if (so == null) yield break;

        // 히트 판정 타겟 설정(HPController 쪽 public 메서드 사용)
        if (hp != null) hp.BeginCardHitTest(foe);

        var centers = (foe == Faction.Player) ? playerCentersPos : enemyCentersPos;
        if (centers == null || centers.Length < 16)
        {
            Debug.LogWarning("[AttackController] centers pos array must have 16 elements.");
            yield break;
        }

        EnsurePoolSize(16);
        bool damageApplied = false;

        if (so.useWaves && so.waves != null && so.waves.Length > 0)
        {
            var accumMask = new bool[16];

            for (int wi = 0; wi < so.waves.Length; wi++)
            {
                var w = so.waves[wi];
                if (w == null) continue;

                var mask = new bool[16];
                var times = new float[16];
                AttackCardSO.ParsePattern16(w.pattern16, mask);
                AttackCardSO.FillTimeline16(w.timeline, times);

                for (int rep = 0; rep < Mathf.Max(1, w.repeat); rep++)
                {
                    if (w.preDelay > 0f) yield return new WaitForSeconds(w.preDelay);

                    bool[] renderMask;
                    if (w.combine == AttackCardSO.WaveCombineMode.Additive)
                    {
                        for (int i = 0; i < 16; i++) accumMask[i] = accumMask[i] || mask[i];
                        renderMask = accumMask;
                    }
                    else
                    {
                        renderMask = mask; // Replace
                        HideAllTiles();
                    }

                    ShowTiles(renderMask, centers);

                    var schedule = BuildSchedule(renderMask, times);
                    float lastT = 0f;
                    foreach (var (idx, tPoint) in schedule)
                    {
                        float wait = Mathf.Max(0f, tPoint - lastT);
                        if (wait > 0f) yield return new WaitForSeconds(wait);
                        lastT = tPoint;

                        if (!damageApplied || !oneHitPerCard)
                        {
                            if (idx < 0 || idx >= centers.Length) continue;
                            Vector3 center = centers[idx]; center.z = tileZ;

                            if (CheckHitNow(center))
                            {
                                ApplyDamageOnce(so, self, foe, ref damageApplied);
                                PlayOnHitFX(so, center);
                            }
                        }
                    }

                    if (w.postDelay > 0f) yield return new WaitForSeconds(w.postDelay);
                }
            }
        }
        else
        {
            var mask = new bool[16];
            var times = new float[16];
            AttackCardSO.ParsePattern16(so.pattern16, mask);
            AttackCardSO.FillTimeline16(so.timeline, times);

            ShowTiles(mask, centers);

            var schedule = BuildSchedule(mask, times);
            float lastT = 0f;
            foreach (var (idx, tPoint) in schedule)
            {
                float wait = Mathf.Max(0f, tPoint - lastT);
                if (wait > 0f) yield return new WaitForSeconds(wait);
                lastT = tPoint;

                if (!damageApplied || !oneHitPerCard)
                {
                    if (idx < 0 || idx >= centers.Length) continue;
                    Vector3 center = centers[idx]; center.z = tileZ;

                    if (CheckHitNow(center))
                    {
                        ApplyDamageOnce(so, self, foe, ref damageApplied);
                        PlayOnHitFX(so, center);
                    }
                }
            }
        }

        HideAllTiles();
    }

    // ---------- 내부 유틸 ----------

    private List<(int idx, float time)> BuildSchedule(bool[] mask, float[] times)
    {
        var list = new List<(int idx, float time)>(16);
        for (int i = 0; i < 16; i++)
        {
            if (mask[i])
            {
                float t = (times != null && times.Length > i) ? Mathf.Max(0f, times[i]) : 0f;
                list.Add((idx: i, time: t));
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

    private void PlayOnHitFX(AttackCardSO so, Vector3 at)
    {
        if (so.sfx) AudioSource.PlayClipAtPoint(so.sfx, at);
        // if (so.cameraShake) ...
    }
}
