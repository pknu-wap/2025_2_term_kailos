using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [SerializeField] private Image image;
    private BattleHandUI owner;
    private int index;

    public void Init(BattleHandUI owner, int index, Sprite sprite)
    {
        this.owner = owner;
        this.index = index;

        if (!image) image = GetComponent<Image>();
        if (image) image.sprite = sprite;

        // 버튼 연결(프리팹에서 연결해둔 경우 중복 연결 안 되도록 체크)
        var btn = GetComponent<Button>();
        if (btn)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick);
        }
    }

    public void OnClick()
    {
        if (owner == null) return;
        owner.OnClickCard(index);
    }
}
