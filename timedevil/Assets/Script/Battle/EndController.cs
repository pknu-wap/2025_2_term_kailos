// Assets/Script/Battle/EndController.cs
using UnityEngine;

public class EndController : MonoBehaviour
{
    [SerializeField] private BattleMenuController menu;
    [SerializeField] private HandUI hand;

    void Reset()
    {
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!hand) hand = FindObjectOfType<HandUI>(true);
    }

    void Update()
    {
        if (!menu) return;

        // 메뉴가 End(2)일 때 E키로 턴 종료
        if (menu.Index == 2 && Input.GetKeyDown(KeyCode.E))
            DoEndTurn();
    }

    public void DoEndTurn()
    {
        // 선택모드/커서 정리 (안전)
        if (hand && hand.IsInSelectMode) hand.ExitSelectMode();

        // 메뉴 입력 잠깐 막기 (실수 방지)
        menu.EnableInput(false);

        // 턴 매니저에 위임
        if (TurnManager.Instance != null)
            TurnManager.Instance.OnPlayerActionCommitted(); // → BeginEnemyTurn()
    }
}
