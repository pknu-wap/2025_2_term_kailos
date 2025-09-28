using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class EnemyAttackController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AttackController attackController;

    [Header("Enemy Pattern")]
    [Tooltip("ICardPattern 구현 타입 이름 (예: Enemy1)")]
    [SerializeField] private string enemyTypeName = "Enemy1";

    /// <summary>TurnManager → EnemyController에서 호출: 공격 1회 연출</summary>
    public IEnumerator ExecuteAttackOnce()
    {
        if (attackController == null)
        {
            Debug.LogWarning("[EnemyAttackController] attackController 미할당");
            yield break;
        }

        if (string.IsNullOrEmpty(enemyTypeName))
            enemyTypeName = "Enemy1";

        // 리플렉션으로 타입 획득
        var t = FindTypeByName(enemyTypeName);
        if (t == null)
        {
            Debug.LogWarning($"[EnemyAttackController] Enemy 타입을 찾을 수 없음: {enemyTypeName}");
            yield break;
        }

        // 임시 GO 에 붙여서 데이터 획득
        var go = new GameObject($"_EnemyPattern_{enemyTypeName}");
        float total = 0f;

        try
        {
            var comp = go.AddComponent(t) as ICardPattern;
            if (comp == null)
            {
                Debug.LogWarning($"[EnemyAttackController] 타입은 찾았으나 ICardPattern이 아님: {enemyTypeName}");
                yield break;
            }

            var timings = comp.Timings ?? new float[16];

            // 왼쪽(플레이어) 패널에 표시
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
