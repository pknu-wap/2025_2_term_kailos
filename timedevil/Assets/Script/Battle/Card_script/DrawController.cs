// DrawController.cs (전체 교체해도 됨)
using System.Collections;
using UnityEngine;

public class DrawController : MonoBehaviour
{
    [Header("Optional VFX (UpDraw only, once)")]
    [SerializeField] private GameObject upDrawParticlePrefab;
    [SerializeField] private float vfxLifetime = 1.2f;

    [Header("Anchors (where to spawn VFX)")]
    [SerializeField] private Transform playerHandAnchor;
    [SerializeField] private Transform enemyHandAnchor;

    [Header("Anime")]
    [SerializeField] private CardAnimeController cardAnime; // 👈 연결

    public IEnumerator Execute(DrawCardSO so, Faction self)
    {
        if (so == null) yield break;

        int actuallyDrawn = 0;

        if (so.drawMode == DrawMode.UpDraw)
        {
            if (self == Faction.Player)
            {
                var deck = BattleDeckRuntime.Instance;
                if (deck != null)
                {
                    // cap 무시 드로우 → 실제 드로우된 장수 반환
                    actuallyDrawn = deck.Draw(so.amount, ignoreHandCap: true);
                }
                else Debug.LogWarning("[DrawController] BattleDeckRuntime is null (player).");
            }
            else
            {
                var enemy = EnemyDeckRuntime.Instance;
                if (enemy != null)
                {
                    actuallyDrawn = enemy.Draw(so.amount, ignoreHandCap: true);
                }
                else Debug.LogWarning("[DrawController] EnemyDeckRuntime is null (enemy).");
            }
        }
        else
        {
            Debug.Log("[DrawController] AntiDraw is not implemented yet.");
        }

        // 드로우 VFX
        if (upDrawParticlePrefab != null)
        {
            Transform anchor = (self == Faction.Player) ? playerHandAnchor : enemyHandAnchor;
            Vector3 pos = anchor ? anchor.position : Vector3.zero;
            var go = Instantiate(upDrawParticlePrefab, pos, Quaternion.identity);
            if (vfxLifetime > 0f) Destroy(go, vfxLifetime);
        }

        // UI가 OnHandChanged로 리빌드된 뒤, 마지막 N장만 애니
        if (actuallyDrawn > 0 && cardAnime != null)
        {
            // 한 프레임 보장 (리빌드 → RectTransform 생성 완료 대기)
            yield return new WaitForEndOfFrame();
            cardAnime.RevealLastNCards(self, actuallyDrawn);
        }

        yield return null;
    }

    public void SetAnchors(Transform player, Transform enemy)
    {
        playerHandAnchor = player;
        enemyHandAnchor = enemy;
    }
}
