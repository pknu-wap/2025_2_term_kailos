// CardUI.cs
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [SerializeField] private Image image;     // 카드 sprite 표시용
    private Button button;                    // 클릭용
    private BattleHandUI owner;               // 부모 UI
    private int index;                        // hand 내 인덱스

    void Awake()
    {
        // 컴포넌트 캐시
        if (!image) image = GetComponent<Image>();
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    /// <summary>BattleHandUI에서 생성 직후 호출</summary>
    public void Init(BattleHandUI owner, int index, Sprite sprite)
    {
        this.owner = owner;
        this.index = index;
        if (image) image.sprite = sprite;
    }

    /// <summary>필요 시 인덱스만 갱신할 때 사용</summary>
    public void SetIndex(int newIndex) => index = newIndex;

    public void OnClick()
    {
        Debug.Log($"[CardUI] 클릭 => index={index}, owner={(owner ? owner.name : "null")}");
        if (owner != null) owner.OnClickCard(index);
        else Debug.LogWarning("[CardUI] owner가 null이라 클릭을 전달 못함");
    }
}
