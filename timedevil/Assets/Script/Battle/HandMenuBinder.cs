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
        // 0=Card, 1=Item, 2=End, 3=Run
        if (!handUI) return;

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
