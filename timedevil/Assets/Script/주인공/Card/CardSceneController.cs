using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class CardSceneController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private Transform cardPanel;   // 보유 카드 영역
    [SerializeField] private Transform deckPanel;   // 덱 영역
    [SerializeField] private Image explainImage;    // 설명(큰 미리보기)
    [SerializeField] private RectTransform selector;// 주황 선택 박스

    [Header("Prefab & Resources")]
    [SerializeField] private GameObject cardSlotPrefab;  // Image 하나만 있는 프리팹
    [SerializeField] private string resourcesFolder = "my_asset"; // Resources/my_asset/<CardId>

    // 내부 상태
    private readonly List<CardSlot> cardSlots = new();
    private readonly List<CardSlot> deckSlots = new();
    private int currentIndex = 0;
    private bool inDeck = false; // false = Card영역, true = Deck영역

    void Start()
    {
        var runtime = CardStateRuntime.Instance;
        var data = runtime != null ? runtime.Data : new CardSaveData();

        var owned = data.owned ?? new List<string>();
        var deck  = data.deck  ?? new List<string>();

        // 1) Card 패널: "보유 - 덱" 차집합만 표시 (이미 덱에 있는 카드는 제외)
        foreach (var id in owned.Where(id => !deck.Contains(id)))
            AddSlotToPanel(cardPanel, cardSlots, id);

        // 2) Deck 패널: 덱 목록 그대로 표시
        foreach (var id in deck)
            AddSlotToPanel(deckPanel, deckSlots, id);

        UpdateSelector();
        UpdateExplain();
    }

void Update()
{
    HandleInput(); // 원래 있던 입력 처리 (카드 선택, 이동 등)

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (!string.IsNullOrEmpty(SceneHistory.LastSceneName))
            {
                Time.timeScale = 1f; // 혹시 멈춰 있으면 풀기
                SceneManager.LoadScene(SceneHistory.LastSceneName);
            }
            else
            {
                Debug.LogWarning("[CardSceneController] 이전 씬 기록이 없습니다.");
            }
            return;
        }
    }


    // ----------------------------------------------------

    void HandleInput()
    {
        // ✅ W: 직전 씬으로 돌아가기 (Card 씬 공통 단축키)
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (!string.IsNullOrEmpty(SceneHistory.LastSceneName))
            {
                Time.timeScale = 1f; // 메뉴 등에서 0으로 멈춰있을 수 있으니 복구
                SceneManager.LoadScene(SceneHistory.LastSceneName);
            }
            else
            {
                Debug.LogWarning("[CardSceneController] 이전 씬 기록이 없습니다.");
            }
            return; // W 처리했으면 더 진행 안 함
        }

        var list = inDeck ? deckSlots : cardSlots;

        if (list.Count == 0)
        {
            // 선택 가능한 슬롯이 없으면 설명/셀렉터 갱신만
            UpdateSelector();
            UpdateExplain();

            // 영역 전환만 허용
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                inDeck = !inDeck;
                currentIndex = 0;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex = (currentIndex + 1) % list.Count;
            UpdateSelector(); UpdateExplain();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex = (currentIndex - 1 + list.Count) % list.Count;
            UpdateSelector(); UpdateExplain();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            inDeck = !inDeck;
            currentIndex = 0;
            UpdateSelector(); UpdateExplain();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (!inDeck)
                MoveCard_toDeck_and_RemoveFromCard();
            else
                MoveCard_toCard_and_RemoveFromDeck(); // 필요 없으면 주석 처리
        }
    }

    // Card → Deck 이동 + Card에서 제거 (요구사항 1번 해결)
    void MoveCard_toDeck_and_RemoveFromCard()
    {
        if (cardSlots.Count == 0) return;

        var slot = cardSlots[currentIndex];
        var id = slot.cardId;
        if (string.IsNullOrEmpty(id)) return;

        // 덱 데이터 갱신(중복 방지)
        var data = CardStateRuntime.Instance.Data;
        data.deck ??= new List<string>();
        if (!data.deck.Contains(id))
        {
            data.deck.Add(id);

            // 덱 UI에 추가
            AddSlotToPanel(deckPanel, deckSlots, id);
        }

        // ✅ Card 패널에서 제거(슬롯 파괴 + 리스트 제거)
        var removedGO = slot.gameObject;
        cardSlots.RemoveAt(currentIndex);
        Destroy(removedGO);

        // 선택 인덱스 보정
        currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, cardSlots.Count - 1));

        UpdateSelector(); UpdateExplain();
    }

    // (옵션) Deck → Card 이동 + Deck에서 제거
    void MoveCard_toCard_and_RemoveFromDeck()
    {
        if (deckSlots.Count == 0) return;

        var slot = deckSlots[currentIndex];
        var id = slot.cardId;
        if (string.IsNullOrEmpty(id)) return;

        // 덱 데이터 갱신
        var data = CardStateRuntime.Instance.Data;
        if (data.deck != null) data.deck.Remove(id);

        // 카드 UI에 추가 (보유 목록에 있다면 단순 표시; 없으면 표시만 해도 무방)
        AddSlotToPanel(cardPanel, cardSlots, id);

        // ✅ Deck 패널에서 제거
        var removedGO = slot.gameObject;
        deckSlots.RemoveAt(currentIndex);
        Destroy(removedGO);

        currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, deckSlots.Count - 1));

        UpdateSelector(); UpdateExplain();
    }

    // ----------------------------------------------------

    void AddSlotToPanel(Transform parent, List<CardSlot> list, string cardId)
    {
        var go = Instantiate(cardSlotPrefab, parent);
        var slot = go.GetComponent<CardSlot>();
        if (!slot) slot = go.AddComponent<CardSlot>();

        var sprite = Resources.Load<Sprite>($"{resourcesFolder}/{cardId}");
        slot.Setup(cardId, sprite);

        list.Add(slot);
    }

    void UpdateSelector()
    {
        var list = inDeck ? deckSlots : cardSlots;
        if (list.Count == 0)
        {
            // 슬롯이 없으면 셀렉터를 잠깐 숨기는 것도 방법 (원하면 selector.gameObject.SetActive(false);)
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, list.Count - 1);
        selector.position = list[currentIndex].transform.position;
    }

    void UpdateExplain()
    {
        var list = inDeck ? deckSlots : cardSlots;
        if (list.Count == 0)
        {
            if (explainImage) explainImage.sprite = null;
            return;
        }

        var slot = list[currentIndex];
        if (explainImage) explainImage.sprite = slot.image ? slot.image.sprite : null;
    }
}
