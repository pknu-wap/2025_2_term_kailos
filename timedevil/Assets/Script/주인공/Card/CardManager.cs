using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public Transform cardParent; // ī�� UI �θ�
    public GameObject cardPrefab; // Image ������Ʈ ���� ������

    void Start()
    {
        // ItemDatabase Ȯ��
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("[CardManager] ItemDatabase.Instance�� ����. MyRoom���� �������� ���� �����ؾ� ��");
            return;
        }

        // ������ �������� UI ī��� ����
        foreach (string itemName in ItemDatabase.Instance.collectedItems)
        {
            Sprite cardSprite = Resources.Load<Sprite>($"my_asset/{itemName}");
            if (cardSprite != null)
            {
                GameObject card = Instantiate(cardPrefab, cardParent);
                card.GetComponent<Image>().sprite = cardSprite;
            }
            else
            {
                Debug.LogWarning($"{itemName} ��������Ʈ�� Resources/my_asset/���� ã�� �� ����");
            }
        }
    }
}
