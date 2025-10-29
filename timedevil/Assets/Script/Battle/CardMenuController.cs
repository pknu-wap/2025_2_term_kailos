using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardMenuController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BattleMenuController menu;   // 포커스 이벤트 소스
    [SerializeField] private RectTransform handPanel;     // 카드 슬롯들이 들어갈 부모
    [SerializeField] private GameObject cardPrefab;       // Image 1개 이상 포함한 프리팹
    [SerializeField] private CardSpriteResolver spriteResolver;

    [Header("Deck Sources (둘 중 하나 사용)")]
    [SerializeField] private BattleDeckRuntime deckRuntime;   // 있으면 사용
    [SerializeField] private List<string> testDeckIds = new(); // 없으면 이 리스트 사용

    [Header("Behavior")]
    [SerializeField] private bool drawOnlyOncePerTurn = true;

    private readonly List<string> handIds = new();
    private bool hasDrawnThisTurn;

    void Awake()
    {
        if (menu != null)
            menu.FocusChanged += OnMenuFocusChanged;
    }

    void OnDestroy()
    {
        if (menu != null)
            menu.FocusChanged -= OnMenuFocusChanged;
    }

    // 턴이 바뀔 때 TurnManager가 호출 예정(지금은 수동으로 Inspector에서 테스트 가능)
    public void OnTurnStart()
    {
        hasDrawnThisTurn = false;
        handIds.Clear();
        // 필요시 이전 손패 UI도 비우고 새로 뽑도록 하려면 다음 줄 주석 해제
        // ClearHandUI();
    }

    private void OnMenuFocusChanged(int index)
    {
        // 0=Card, 1=Item, 2=Run
        if (index == 0)
        {
            EnsureHandDrawn();
            SetHandVisible(true);
        }
        else if (index == 1)
        {
            // Item 포커스여도 Hand는 유지 (요구사항)
            SetHandVisible(true);
        }
        else // Run
        {
            SetHandVisible(false);
        }
    }

    private void EnsureHandDrawn()
    {
        if (handIds.Count > 0 && drawOnlyOncePerTurn && hasDrawnThisTurn)
            return;

        handIds.Clear();
        var deck = GetDeckIds();
        DrawRandomUpTo(deck, 3, handIds);
        PopulateHandUI();
        hasDrawnThisTurn = true;
    }

    private List<string> GetDeckIds()
    {
        // BattleDeckRuntime가 있으면 거기서 카드 ID 리스트를 뽑아온다.
        // (프로젝트 구조에 맞춰 이 부분을 수정하세요)
        if (deckRuntime != null && deckRuntime.deck != null && deckRuntime.deck.Count > 0)
        {
            // deckRuntime.deck 이 string 리스트라는 가정.
            // 만약 카드 오브젝트라면 .id 같은 필드로 변환 필요.
            return deckRuntime.deck;
        }
        return testDeckIds;
    }

    private void DrawRandomUpTo(List<string> source, int count, List<string> outList)
    {
        if (source == null || source.Count == 0) return;

        // 간단 랜덤 뽑기(중복 허용 X)
        List<int> indices = new List<int>(source.Count);
        for (int i = 0; i < source.Count; i++) indices.Add(i);

        for (int n = 0; n < count && indices.Count > 0; n++)
        {
            int k = Random.Range(0, indices.Count);
            int idx = indices[k];
            indices.RemoveAt(k);
            outList.Add(source[idx]);
        }
    }

    private void PopulateHandUI()
    {
        if (handPanel == null || cardPrefab == null) return;

        ClearHandUI();

        foreach (var id in handIds)
        {
            var go = Instantiate(cardPrefab, handPanel);
            go.name = $"Card_{id}";
            var img = go.GetComponentInChildren<Image>();
            if (img != null && spriteResolver != null)
                img.sprite = spriteResolver.GetSprite(id);
        }
    }

    private void ClearHandUI()
    {
        for (int i = handPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(handPanel.GetChild(i).gameObject);
        }
    }

    private void SetHandVisible(bool on)
    {
        if (!handPanel) return;
        handPanel.gameObject.SetActive(on);
    }
}
