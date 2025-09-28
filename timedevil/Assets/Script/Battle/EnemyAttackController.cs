using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class EnemyAttackController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AttackController attackController;

    [Header("Enemy Pattern")]
    [Tooltip("ICardPattern ���� Ÿ�� �̸� (��: Enemy1)")]
    [SerializeField] private string enemyTypeName = "Enemy1";

    /// <summary>TurnManager �� EnemyController���� ȣ��: ���� 1ȸ ����</summary>
    public IEnumerator ExecuteAttackOnce()
    {
        if (attackController == null)
        {
            Debug.LogWarning("[EnemyAttackController] attackController ���Ҵ�");
            yield break;
        }

        if (string.IsNullOrEmpty(enemyTypeName))
            enemyTypeName = "Enemy1";

        // ���÷������� Ÿ�� ȹ��
        var t = FindTypeByName(enemyTypeName);
        if (t == null)
        {
            Debug.LogWarning($"[EnemyAttackController] Enemy Ÿ���� ã�� �� ����: {enemyTypeName}");
            yield break;
        }

        // �ӽ� GO �� �ٿ��� ������ ȹ��
        var go = new GameObject($"_EnemyPattern_{enemyTypeName}");
        float total = 0f;

        try
        {
            var comp = go.AddComponent(t) as ICardPattern;
            if (comp == null)
            {
                Debug.LogWarning($"[EnemyAttackController] Ÿ���� ã������ ICardPattern�� �ƴ�: {enemyTypeName}");
                yield break;
            }

            var timings = comp.Timings ?? new float[16];

            // ����(�÷��̾�) �гο� ǥ��
            attackController.ShowPattern(comp.Pattern16, timings, AttackController.Panel.Player);

            total = attackController.GetSequenceDuration(timings);
        }
        finally
        {
            Destroy(go);
        }

        if (total > 0f) yield return new WaitForSeconds(total);
    }

    public void SetEnemyType(string typeName)
    {
        enemyTypeName = typeName;
    }

    static Type FindTypeByName(string typeName)
    {
        var asm = typeof(EnemyAttackController).Assembly;
        return asm.GetTypes().FirstOrDefault(t => t.Name == typeName && typeof(MonoBehaviour).IsAssignableFrom(t));
    }
}
