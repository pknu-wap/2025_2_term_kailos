using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public Transform cardParent; // 카드 UI 부모
    public GameObject cardPrefab; // Image 컴포넌트 포함 프리팹

    void Start()
    {
        // ItemDatabase 확인
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("[CardManager] ItemDatabase.Instance가 없음. MyRoom에서 아이템을 먼저 습득해야 함");
            return;
        }

        // 수집된 아이템을 UI 카드로 생성
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
                Debug.LogWarning($"{itemName} 스프라이트를 Resources/my_asset/에서 찾을 수 없음");
            }
        }
    }
}
