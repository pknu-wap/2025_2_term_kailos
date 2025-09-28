using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private EnemyMoveController moveController;
    [SerializeField] private EnemyAttackController attackController;

    [Header("Weights")]
    [Range(0f, 1f)]
    [Tooltip("�̵� Ȯ�� (1-�� �� = ���� Ȯ��)")]
    public float moveProbability = 0.5f;

    /// <summary>�� �Ŵ����� ȣ��: �� �ൿ 1ȸ ����</summary>
    public IEnumerator ExecuteOneAction()
    {
        if (moveController == null && attackController == null)
        {
            Debug.LogWarning("[EnemyController] ���� ��Ʈ�ѷ��� ������� ����");
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
            // ������ ���� �� �ٸ� �� fallback
            if (moveController != null) yield return moveController.ExecuteMoveOneStep();
        }
    }
}
