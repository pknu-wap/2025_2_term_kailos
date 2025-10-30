using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CardUI : MonoBehaviour
{
    [SerializeField] private Image image;   // ī�� �׸�ǥ��
    private string cardId;
    private int indexInHand;
    private Action<string, int> onClick;

    public string CardId => cardId;
    public int Index => indexInHand;

    /// <summary>
    /// ī�� �� ĭ�� �����Ѵ�.
    /// </summary>
    /// <param name="id">ī�� ID (��: "Card3")</param>
    /// <param name="sprite">ǥ���� ��������Ʈ(null�̸� �� ī��ó�� ����)</param>
    /// <param name="index">���� �� �ε���</param>
    /// <param name="clicked">Ŭ�� �ݹ�(�ɼ�)</param>
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

        // ��ư ����
        var btn = GetComponent<Button>();
        if (!btn) btn = gameObject.AddComponent<Button>();
        btn.transition = Selectable.Transition.ColorTint; // �⺻
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick?.Invoke(cardId, indexInHand));
    }
}
