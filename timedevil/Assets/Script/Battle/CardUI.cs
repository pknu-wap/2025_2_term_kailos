using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class CardUI : MonoBehaviour, IPointerClickHandler 
{ 
    [Header("Refs")]
    [SerializeField] private Image art; // 카드 그림 표시
    [SerializeField] private Button button; // 클릭용(없어도 동작, 있으면 onClick 사용)
    [SerializeField] private Image highlight; // 선택/호버용(선택 아님: 지금은 비활성 유지)
    
    private BattleHandUI owner; 
    private int handIndex = -1; 
    /// <summary> 
    /// BattleHandUI에서 카드 한 장을 구성할 때 호출. 
    /// </summary> 
    /// <param name="owner">소유자(콜백 보낼 곳)</param> 
    /// <param name="index">손패 인덱스</param> 
    /// <param name="sprite">카드 스프라이트(없으면 빈 카드)</param> 
    public void Init(BattleHandUI owner, int index, Sprite sprite) 
    { 
        this.owner = owner; 
        this.handIndex = index; 
        
        if (!art) art = GetComponentInChildren<Image>(); 
        if (!button) button = GetComponent<Button>(); 
        
        if (art) 
        { 
            art.sprite = sprite; 
            art.preserveAspect = true; 
            art.raycastTarget = true; 
        } 
        if (highlight) highlight.enabled = false; 
        if (button) 
        { 
            button.onClick.RemoveAllListeners(); 
            button.onClick.AddListener(() => 
            owner?.OnClickCard(this.handIndex)); 
        } 
    } 
    // 버튼이 없어도 클릭 가능하도록
    public void OnPointerClick(PointerEventData eventData) 
    { 
        owner?.OnClickCard(handIndex); 
    } 
}