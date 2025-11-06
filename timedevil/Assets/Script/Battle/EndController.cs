// Assets/Script/Battle/EndController.cs
using UnityEngine;

public class EndController : MonoBehaviour
{
    [SerializeField] private BattleMenuController menu;

    void Reset()
    {
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
    }

    void Update()
    {
        if (!menu) return;

        // 메뉴가 End(2)일 때 E키로 턴 종료
        if (menu.Index == 2 && Input.GetKeyDown(KeyCode.E))
        {
            // 플레이어가 End를 확정 → 턴 매니저에 알림
            if (TurnManager.Instance != null)
            {
                Debug.Log("[EndController] End 선택 → 적 턴으로 넘김");
                TurnManager.Instance.OnPlayerActionCommitted();
            }
        }
    }
}
