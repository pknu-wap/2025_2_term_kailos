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

    public IEnumerator Execute(DrawCardSO so, Faction self)
    {
        if (so == null) yield break;

        if (so.drawMode == DrawMode.UpDraw)
        {
            if (self == Faction.Player)
            {
                var deck = BattleDeckRuntime.Instance;
                if (deck != null) deck.Draw(so.amount, ignoreHandCap: true); // ✅ cap 무시
                else Debug.LogWarning("[DrawController] BattleDeckRuntime is null (player).");
            }
            else
            {
                var enemy = EnemyDeckRuntime.Instance;
                if (enemy != null) enemy.Draw(so.amount, ignoreHandCap: true); // ✅ cap 무시
                else Debug.LogWarning("[DrawController] EnemyDeckRuntime is null (enemy).");
            }
        }
        else
        {
            Debug.Log("[DrawController] AntiDraw is not implemented yet.");
        }

        if (upDrawParticlePrefab != null)
        {
            Transform anchor = (self == Faction.Player) ? playerHandAnchor : enemyHandAnchor;
            Vector3 pos = anchor ? anchor.position : Vector3.zero;
            var go = Instantiate(upDrawParticlePrefab, pos, Quaternion.identity);
            if (vfxLifetime > 0f) Destroy(go, vfxLifetime);
        }

        yield return null;
    }

    public void SetAnchors(Transform player, Transform enemy)
    {
        playerHandAnchor = player;
        enemyHandAnchor = enemy;
    }
}
