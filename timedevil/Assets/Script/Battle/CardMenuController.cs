using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardMenuController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BattleMenuController menu;   // 포커스 이벤트 소스 (onFocusChanged)
    [SerializeField] private RectTransform handPanel;     // 카드가 깔릴 부모(가로로 나열)
    [SerializeField] private GameObject cardPrefab;       // Image 포함 프리팹(가로 1장)
    [SerializeField] private CardSpriteResolver spriteResolver;

    [Header("Deck Sources (둘 중 하나 사용)")]
    [SerializeField] private BattleDeckRuntime deckRuntime;   // 있으면 사용
    [SerializeField] private List<string> testDeckIds = new(); // 없으면 이 리스트 사용

    [Header("Behavior")]
    [SerializeField] private bool drawOnlyOncePerTurn = true;

    private readonly List<string> handIds = new();
    private bool hasDrawnThisTurn;

    void Reset()
    {
        if (!menu) menu = FindObjectOfType<BattleMenuController>();
        if (!handPanel)
        {
            var handGO = GameObject.Find("Hand");
            if (handGO) handPanel = handGO.GetComponent<RectTransform>();
        }
    }

    void OnEnable()
    {
        if (!menu) menu = FindObjectOfType<BattleMenuController>();
        if (menu) menu.onFocusChanged.AddListener(OnMenuFocusChanged);

        // 씬 시작/활성 시 현재 포커스로 즉시 반영
        if (menu) OnMenuFocusChanged(menu.Index);
    }

    void OnDisable()
    {
        if (menu) menu.onFocusChanged.RemoveListener(OnMenuFocusChanged);
    }

    /// <summary>턴 시작 시(턴 매니저에서 호출해 주세요)</summary>
    public void OnTurnStart()
    {
        hasDrawnThisTurn = false;
        handIds.Clear();
        // 필요하면 이하 주석 해제
        // ClearHandUI();
        // EnsureHandDrawn(); // 턴 시작과 동시에 3장 준비하고 싶다면
    }

    // 0=Card, 1=Item, 2=Run
    private void OnMenuFocusChanged(int index)
    {
        if (index == 0)             // Card 포커스
        {
            EnsureHandDrawn();
            SetHandVisible(true);
        }
        else if (index == 1)        // Item 포커스(Hand 유지)
        {
            SetHandVisible(true);
        }
        else                        // Run 포커스(Hand 숨김)
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
        if (deckRuntime != null && deckRuntime.deck != null && deckRuntime.deck.Count > 0)
            return deckRuntime.deck; // deck이 string 리스트라는 가정
        return testDeckIds;
    }

    private void DrawRandomUpTo(List<string> source, int count, List<string> outList)
    {
        if (source == null || source.Count == 0) return;

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
        if (!handPanel) return;
        for (int i = handPanel.childCount - 1; i >= 0; i--)
            Destroy(handPanel.GetChild(i).gameObject);
    }

    private void SetHandVisible(bool on)
    {
        if (!handPanel) return;
        handPanel.gameObject.SetActive(on);
    }
}
