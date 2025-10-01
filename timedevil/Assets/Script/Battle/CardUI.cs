using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [HideInInspector] public int handIndex;              // BattleHandUI�� ����
    [HideInInspector] public string cardId;              // �����ִ� ī��ID
    public Image image;                                  // ī�� ���� ������/UI �̹���

    System.Action<int> onClick;                          // BattleHandUI�� ���

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
        Debug.Log($"카드 클릭됨: {cardId}");
    // BattleDeckRuntime.Instance.UseCardToBottom(index); 이런 동작을 여기서 호출 가능
    }
}
