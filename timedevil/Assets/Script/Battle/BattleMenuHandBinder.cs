using UnityEngine;
public class BattleMenuHandBinder : MonoBehaviour { 
    [Header("Sources")][SerializeField] private BattleMenuController menu; 
    // ��Ŀ�� �̺�Ʈ �ҽ�
    [Header("Targets")] [SerializeField] private BattleHandUI hand; 
    // ���� �г� ��Ʈ�ѷ�(Hand ������Ʈ)
    [SerializeField] private bool applyOnEnable = true; 
    void Reset() 
    { 
        if (!menu) menu = FindObjectOfType<BattleMenuController>(includeInactive: true); 
        if (!hand) hand = FindObjectOfType<BattleHandUI>(includeInactive: true); 
    } 
    void OnEnable() 
    { 
        if (!menu) menu = FindObjectOfType<BattleMenuController>(includeInactive: true); 
        if (!hand) hand = FindObjectOfType<BattleHandUI>(includeInactive: true); 
        if (menu != null) 
            menu.OnMenuFocusChanged += HandleFocus; 
        if (applyOnEnable) { 
            int cur = (menu != null) ? menu.CurrentIndex : 0; HandleFocus(cur); 
        } 
    } 
    void OnDisable() 
    { 
        if (menu != null) menu.OnMenuFocusChanged -= HandleFocus; 
    } /// <summary>0=Card, 1=Item, 2=Run</summary> 
    private void HandleFocus(int index) 
    { 
        if (!hand) return; 
        if (index == 0) 
        { 
            // Card ��Ŀ��: ���и� ���� ����ȭ�ؼ� �����ֱ�
            hand.SyncFromDeckAndShow(); } 
        else 
        { 
            // Item/Run ��Ŀ��: ī��鸸 ����(Hand ������Ʈ�� �״��)
            hand.HideCards(); 
        } 
    } 
}