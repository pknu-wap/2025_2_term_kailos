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

        // ���� ��Ŀ���� �ٷ� �ݿ�
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

        // 0=Card �� ���� ī�� ǥ��, �������� ����
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

        // Card�� E�� ���õǾ��� ���� ���ø�� ����
        if (idx == 0)
        {
            hand.EnterSelectMode();
            Debug.Log("[Bridge] Hand select mode entered.");
        }
    }
}
