// Assets/Script/Battle/EnemyTurnController.cs
using System.Collections;
using UnityEngine;

public class EnemyTurnController : MonoBehaviour
{
    [Header("Countdown")]
    [SerializeField] private int countdownSeconds = 5;

    /// <summary>
    /// 적 턴 연출: 5→1 카운트 다운만 수행(디버그 로그).
    /// UI 입력 제한/카드 비활성은 TurnManager에서 처리.
    /// </summary>
    public IEnumerator RunTurn()
    {
        int t = Mathf.Max(1, countdownSeconds);
        while (t > 0)
        {
            Debug.Log($"[EnemyTurn] {t}");
            yield return new WaitForSeconds(1f);
            t--;
        }

        // 필요하면 여기서 추가 연출/AI 로직을 넣고 끝낸다.
    }
}
