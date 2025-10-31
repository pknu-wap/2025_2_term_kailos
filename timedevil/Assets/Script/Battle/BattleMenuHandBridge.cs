using UnityEngine;

public class BattleMenuHandBridge : MonoBehaviour
{
    [SerializeField] private BattleMenuController menu;
    [SerializeField] private HandUI hand;

    void Reset()
    {
        if (!menu) menu = FindObjectOfType<BattleMenuController>(includeInactive: true);
        if (!hand) hand = FindObjectOfType<HandUI>(includeInactive: true);
    }

    void OnEnable()
    {
        if (!menu) menu = FindObjectOfType<BattleMenuController>(includeInactive: true);
        if (!hand) hand = FindObjectOfType<HandUI>(includeInactive: true);

        if (menu)
        {
            menu.onFocusChanged.AddListener(OnFocus);
            menu.onSubmit.AddListener(OnSubmit);
        }

        // 현재 포커스를 바로 반영
        OnFocus(menu ? menu.Index : 0);
    }

    void OnDisable()
    {
        if (menu)
        {
            menu.onFocusChanged.RemoveListener(OnFocus);
            menu.onSubmit.RemoveListener(OnSubmit);
        }
    }

    private void OnFocus(int idx)
    {
        if (!hand) return;

        // 0=Card 일 때만 카드 표시, 나머지는 숨김
        if (idx == 0)
        {
            hand.RebuildFromHand();
            hand.ShowCards();
        }
        else
        {
            hand.HideCards();
        }
    }

    private void OnSubmit(int idx)
    {
        if (!hand) return;

        // Card가 E로 선택되었을 때만 선택모드 진입
        if (idx == 0)
        {
            hand.EnterSelectMode();
            Debug.Log("[Bridge] Hand select mode entered.");
        }
    }
}
