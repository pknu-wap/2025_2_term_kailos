using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private EnemyMoveController moveController;
    [SerializeField] private EnemyAttackController attackController;

    [Header("Weights")]
    [Range(0f, 1f)]
    [Tooltip("이동 확률 (1-이 값 = 공격 확률)")]
    public float moveProbability = 0.5f;

    /// <summary>턴 매니저가 호출: 적 행동 1회 실행</summary>
    public IEnumerator ExecuteOneAction()
    {
        if (moveController == null && attackController == null)
        {
            Debug.LogWarning("[EnemyController] 하위 컨트롤러가 연결되지 않음");
            yield break;
        }

        bool doMove = Random.value < moveProbability;

        if (doMove && moveController != null)
        {
            yield return moveController.ExecuteMoveOneStep();
        }
        else if (attackController != null)
        {
            yield return attackController.ExecuteAttackOnce();
        }
        else
        {
            // 한쪽이 없을 때 다른 쪽 fallback
            if (moveController != null) yield return moveController.ExecuteMoveOneStep();
        }
    }
}
