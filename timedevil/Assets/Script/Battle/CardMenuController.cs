using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardMenuController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BattleMenuController menu;   // ��Ŀ�� �̺�Ʈ �ҽ�
    [SerializeField] private RectTransform handPanel;     // ī�� ���Ե��� �� �θ�
    [SerializeField] private GameObject cardPrefab;       // Image 1�� �̻� ������ ������
    [SerializeField] private CardSpriteResolver spriteResolver;

    [Header("Deck Sources (�� �� �ϳ� ���)")]
    [SerializeField] private BattleDeckRuntime deckRuntime;   // ������ ���
    [SerializeField] private List<string> testDeckIds = new(); // ������ �� ����Ʈ ���

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

    // ���� �ٲ� �� TurnManager�� ȣ�� ����(������ �������� Inspector���� �׽�Ʈ ����)
    public void OnTurnStart()
    {
        hasDrawnThisTurn = false;
        handIds.Clear();
        // �ʿ�� ���� ���� UI�� ���� ���� �̵��� �Ϸ��� ���� �� �ּ� ����
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
            // Item ��Ŀ������ Hand�� ���� (�䱸����)
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
        // BattleDeckRuntime�� ������ �ű⼭ ī�� ID ����Ʈ�� �̾ƿ´�.
        // (������Ʈ ������ ���� �� �κ��� �����ϼ���)
        if (deckRuntime != null && deckRuntime.deck != null && deckRuntime.deck.Count > 0)
        {
            // deckRuntime.deck �� string ����Ʈ��� ����.
            // ���� ī�� ������Ʈ��� .id ���� �ʵ�� ��ȯ �ʿ�.
            return deckRuntime.deck;
        }
        return testDeckIds;
    }

    private void DrawRandomUpTo(List<string> source, int count, List<string> outList)
    {
        if (source == null || source.Count == 0) return;

        // ���� ���� �̱�(�ߺ� ��� X)
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
