using UnityEngine;

public class BattleBootstrap : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HPUIBinder hpUIBinder;   // Canvas�� ���� HPUIBinder
    [SerializeField] private GameObject enemyHost;     // hierarchy�� 'none' ������Ʈ

    void Start()
    {
        var enemyName = (SceneLoadContext.Instance != null &&
                         !string.IsNullOrEmpty(SceneLoadContext.Instance.pendingEnemyName))
                        ? SceneLoadContext.Instance.pendingEnemyName
                        : "Enemy1";

        Debug.Log($"[BattleBootstrap] enemyName(raw)='{enemyName}'");

        // 1) Ÿ�� ã�� ���̱� (��ҹ��� ����)
        var enemyComp = EnemyFactory.AttachEnemyByName(enemyHost.gameObject, enemyName);
        Debug.Log($"[BattleBootstrap] attached enemy type = {(enemyComp ? enemyComp.GetType().Name : "<NULL>")}");

        // 2) �ʵ� Ȯ��
        var t = enemyComp ? enemyComp.GetType() : null;
        var fCur = t?.GetField("currentHP", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var fMax = t?.GetField("maxHP", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        Debug.Log($"[BattleBootstrap] fields present? currentHP={(fCur != null)}, maxHP={(fMax != null)}");

        if (enemyComp && fCur != null && fMax != null)
        {
            var cur = fCur.GetValue(enemyComp);
            var max = fMax.GetValue(enemyComp);
            Debug.Log($"[BattleBootstrap] enemy HP values = {cur} / {max}");
        }

        // 3) ���δ� ���� + ����
        hpUIBinder.BindEnemy(enemyComp as MonoBehaviour);
        hpUIBinder.Refresh();
    }

}
