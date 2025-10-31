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

        var btn = GetComponent<Button>();
        if (btn)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (owner == null) return;
        owner.OnClickCard(index);   // BattleHandUI에서 받아 처리
    }
}
