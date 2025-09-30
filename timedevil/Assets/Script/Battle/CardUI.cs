using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [HideInInspector] public int handIndex;              // BattleHandUI가 세팅
    [HideInInspector] public string cardId;              // 보여주는 카드ID
    public Image image;                                  // 카드 작은 아이콘/UI 이미지

    System.Action<int> onClick;                          // BattleHandUI가 등록

    public void Setup(int index, string id, Sprite sprite, System.Action<int> click)
    {
        handIndex = index;
        cardId = id;
        if (image) image.sprite = sprite;
        onClick = click;
    }

    public void OnClick()
    {
        onClick?.Invoke(handIndex);
    }
}
