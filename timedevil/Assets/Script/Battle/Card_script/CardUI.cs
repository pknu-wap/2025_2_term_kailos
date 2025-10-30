using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CardUI : MonoBehaviour
{
    [SerializeField] private Image image;   // 카드 그림표시
    private string cardId;
    private int indexInHand;
    private Action<string, int> onClick;

    public string CardId => cardId;
    public int Index => indexInHand;

    /// <summary>
    /// 카드 한 칸을 세팅한다.
    /// </summary>
    /// <param name="id">카드 ID (예: "Card3")</param>
    /// <param name="sprite">표시할 스프라이트(null이면 빈 카드처럼 보임)</param>
    /// <param name="index">손패 내 인덱스</param>
    /// <param name="clicked">클릭 콜백(옵션)</param>
    public void Init(string id, Sprite sprite, int index, Action<string, int> clicked = null)
    {
        cardId = id;
        indexInHand = index;
        onClick = clicked;

        if (!image) image = GetComponent<Image>();
        if (image)
        {
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = true;
        }

        // 버튼 보장
        var btn = GetComponent<Button>();
        if (!btn) btn = gameObject.AddComponent<Button>();
        btn.transition = Selectable.Transition.ColorTint; // 기본
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick?.Invoke(cardId, indexInHand));
    }
}
