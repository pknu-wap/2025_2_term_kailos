using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardSceneController : MonoBehaviour
{
    [Header("UI Panels")]
    public Transform cardPanel;   // 얻은 카드 영역
    public Transform deckPanel;   // 덱 카드 영역
    public Image explainImage;    // 설명 영역 큰 이미지
    public RectTransform selector; // 주황색 select_card

    [Header("Prefabs")]
    public GameObject cardSlotPrefab; // 카드 슬롯 프리팹 (Image 1개만 있는 것)

    private List<CardSlot> cardSlots = new List<CardSlot>();
    private List<CardSlot> deckSlots = new List<CardSlot>();
    private int currentIndex = 0;
    private bool inDeck = false; // 현재 커서가 덱 영역에 있는지?

    void Start()
    {
        LoadOwnedCards();
        UpdateSelector();
    }

    void Update()
    {
        HandleInput();
        UpdateExplain();
    }

    void LoadOwnedCards()
    {
        var data = CardStateRuntime.Instance.Data;
        foreach (var cardId in data.owned)
        {
            GameObject go = Instantiate(cardSlotPrefab, cardPanel);
            var slot = go.AddComponent<CardSlot>();
            slot.cardId = cardId;
            slot.image = go.GetComponent<Image>();

            Sprite sprite = Resources.Load<Sprite>($"my_asset/{cardId}");
            if (sprite) slot.image.sprite = sprite;

            cardSlots.Add(slot);
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex++;
            if (currentIndex >= (inDeck ? deckSlots.Count : cardSlots.Count))
                currentIndex = 0;
            UpdateSelector();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = (inDeck ? deckSlots.Count : cardSlots.Count) - 1;
            UpdateSelector();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            inDeck = !inDeck;
            currentIndex = 0;
            UpdateSelector();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!inDeck && cardSlots.Count > 0) // 카드 → 덱
            {
                var slot = cardSlots[currentIndex];
                if (!CardStateRuntime.Instance.Data.deck.Contains(slot.cardId))
                {
                    CardStateRuntime.Instance.Data.deck.Add(slot.cardId);

                    GameObject go = Instantiate(cardSlotPrefab, deckPanel);
                    var deckSlot = go.AddComponent<CardSlot>();
                    deckSlot.cardId = slot.cardId;
                    deckSlot.image = go.GetComponent<Image>();
                    deckSlot.image.sprite = slot.image.sprite;

                    deckSlots.Add(deckSlot);
                }
            }
            else if (inDeck && deckSlots.Count > 0) // 덱 → 카드 (삭제)
            {
                var slot = deckSlots[currentIndex];
                CardStateRuntime.Instance.Data.deck.Remove(slot.cardId);

                Destroy(slot.gameObject);
                deckSlots.RemoveAt(currentIndex);
                currentIndex = Mathf.Clamp(currentIndex, 0, deckSlots.Count - 1);
            }
        }
    }

    void UpdateSelector()
    {
        if (!inDeck && cardSlots.Count > 0)
            selector.position = cardSlots[currentIndex].transform.position;
        else if (inDeck && deckSlots.Count > 0)
            selector.position = deckSlots[currentIndex].transform.position;
    }

    void UpdateExplain()
    {
        string cardId = null;
        if (!inDeck && cardSlots.Count > 0) cardId = cardSlots[currentIndex].cardId;
        else if (inDeck && deckSlots.Count > 0) cardId = deckSlots[currentIndex].cardId;

        if (!string.IsNullOrEmpty(cardId))
        {
            var sprite = Resources.Load<Sprite>($"my_asset/{cardId}");
            explainImage.sprite = sprite;
        }
        else
        {
            explainImage.sprite = null;
        }
    }
}

public class CardSlot : MonoBehaviour
{
    public string cardId;
    public Image image;
}
