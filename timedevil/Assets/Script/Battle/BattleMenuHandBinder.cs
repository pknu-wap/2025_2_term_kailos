using UnityEngine;
public class BattleMenuHandBinder : MonoBehaviour { 
    [Header("Sources")][SerializeField] private BattleMenuController menu; 
    // 포커스 이벤트 소스
    [Header("Targets")] [SerializeField] private BattleHandUI hand; 
    // 손패 패널 컨트롤러(Hand 오브젝트)
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
            // Card 포커스: 손패를 덱과 동기화해서 보여주기
            hand.SyncFromDeckAndShow(); } 
        else 
        { 
            // Item/Run 포커스: 카드들만 숨김(Hand 오브젝트는 그대로)
            hand.HideCards(); 
        } 
    } 
}