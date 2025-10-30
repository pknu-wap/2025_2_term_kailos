using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardMenuController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BattleMenuController menu;   // ��Ŀ�� �̺�Ʈ �ҽ� (onFocusChanged)
    [SerializeField] private RectTransform handPanel;     // ī�尡 �� �θ�(���η� ����)
    [SerializeField] private GameObject cardPrefab;       // Image ���� ������(���� 1��)
    [SerializeField] private CardSpriteResolver spriteResolver;

    [Header("Deck Sources (�� �� �ϳ� ���)")]
    [SerializeField] private BattleDeckRuntime deckRuntime;   // ������ ���
    [SerializeField] private List<string> testDeckIds = new(); // ������ �� ����Ʈ ���

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

        // �� ����/Ȱ�� �� ���� ��Ŀ���� ��� �ݿ�
        if (menu) OnMenuFocusChanged(menu.Index);
    }

    void OnDisable()
    {
        if (menu) menu.onFocusChanged.RemoveListener(OnMenuFocusChanged);
    }

    /// <summary>�� ���� ��(�� �Ŵ������� ȣ���� �ּ���)</summary>
    public void OnTurnStart()
    {
        hasDrawnThisTurn = false;
        handIds.Clear();
        // �ʿ��ϸ� ���� �ּ� ����
        // ClearHandUI();
        // EnsureHandDrawn(); // �� ���۰� ���ÿ� 3�� �غ��ϰ� �ʹٸ�
    }

    // 0=Card, 1=Item, 2=Run
    private void OnMenuFocusChanged(int index)
    {
        if (index == 0)             // Card ��Ŀ��
        {
            EnsureHandDrawn();
            SetHandVisible(true);
        }
        else if (index == 1)        // Item ��Ŀ��(Hand ����)
        {
            SetHandVisible(true);
        }
        else                        // Run ��Ŀ��(Hand ����)
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
            return deckRuntime.deck; // deck�� string ����Ʈ��� ����
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
