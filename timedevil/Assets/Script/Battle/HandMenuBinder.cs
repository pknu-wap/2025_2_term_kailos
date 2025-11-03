using UnityEngine;

public class HandMenuBinder : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private BattleMenuController menu;

    [Header("Targets")]
    [SerializeField] private HandUI handUI;

    void Reset()
    {
        if (!menu) menu = FindObjectOfType<BattleMenuController>(includeInactive: true);
        if (!handUI) handUI = FindObjectOfType<HandUI>(includeInactive: true);
    }

    void OnEnable()
    {
        if (!menu) menu = FindObjectOfType<BattleMenuController>(includeInactive: true);
        if (!handUI) handUI = FindObjectOfType<HandUI>(includeInactive: true);

        if (menu) menu.onFocusChanged.AddListener(OnFocusChanged);

        // 시작 시 현재 포커스 반영
        var cur = menu ? menu.Index : 0;
        OnFocusChanged(cur);
    }

    void OnDisable()
    {
        if (menu) menu.onFocusChanged.RemoveListener(OnFocusChanged);
    }

    private void OnFocusChanged(int index)
    {
        if (!handUI) return;

        // 선택 모드 중에는 절대 건드리지 않음 (오케스트레이션/ShowCard 중 섞임 방지)
        if (handUI.IsInSelectMode)
            return;

        // 0=Card 일 때만 보여주고, 나머지는 숨김
        if (index == 0)
        {
            handUI.RebuildFromHand();
            handUI.ShowCards();
        }
        else
        {
            handUI.HideCards();
        }
    }
}
