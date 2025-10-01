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
        if (image) image.sprite = sprite;
    }

    // Button OnClick 에 연결
    public void OnClick()
    {
        owner?.OnClickCard(index);
    }
}
