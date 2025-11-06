using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button cardOpenButton;
    [SerializeField] private GameObject cardGroup;
    [SerializeField] private Image cardImage;
    [SerializeField] private Button cardImageButton;

    [Header("Attack")]
    [SerializeField] private AttackController attackController;

    [Header("Resources")]
    [SerializeField] private string resourcesFolder = "my_asset";

    private string _currentCardId;
    private CardSaveData _deckData;

    void Awake()
    {
        if (cardOpenButton)
        {
            cardOpenButton.onClick.RemoveAllListeners();
            cardOpenButton.onClick.AddListener(OpenCardUI);
        }
        if (cardImageButton)
        {
            cardImageButton.onClick.RemoveAllListeners();
            cardImageButton.onClick.AddListener(UseCurrentCard);
        }
        if (cardGroup) cardGroup.SetActive(false);
    }

    void Start()
    {
        // ✅ 런타임 캐시 우선
        var rt = CardStateRuntime.Instance;
        _deckData = (rt != null) ? rt.Data : CardSaveStore.Load();

        // 덱이 비었으면 버튼 비활성화
        if (_deckData == null || _deckData.deck == null || _deckData.deck.Count == 0)
        {
            Debug.Log("[CardController] 덱이 비어 있음 → 카드 사용 비활성화");
            if (cardOpenButton) cardOpenButton.interactable = false;
            return;
        }

        // 첫 장을 현재 카드로
        _currentCardId = _deckData.deck[0];
        RefreshCardIcon();
        if (cardOpenButton) cardOpenButton.interactable = true;
    }

    void OpenCardUI()
    {
        if (string.IsNullOrEmpty(_currentCardId)) return;
        if (cardGroup) cardGroup.SetActive(true);
    }

    void CloseCardUI()
    {
        if (cardGroup) cardGroup.SetActive(false);
    }

    void RefreshCardIcon()
    {
        if (!cardImage) return;
        var sprite = Resources.Load<Sprite>($"{resourcesFolder}/{_currentCardId}");
        cardImage.sprite = sprite; // 못 찾으면 null → 빈 이미지
    }

    void UseCurrentCard()
    {
        if (string.IsNullOrEmpty(_currentCardId) || attackController == null)
        {
            CloseCardUI();
            return;
        }

        var t = FindTypeByName(_currentCardId);
        if (t == null)
        {
            Debug.LogWarning($"[CardController] 카드 타입을 찾지 못함: '{_currentCardId}'");
            CloseCardUI();
            return;
        }

        var go = new GameObject($"_PlayerCard_{_currentCardId}");
        try
        {
            var comp = go.AddComponent(t) as ICardPattern;
            if (comp == null) { CloseCardUI(); return; }

            var timings = comp.Timings ?? new float[16];
            attackController.ShowPattern(comp.Pattern16, timings, AttackController.Panel.Enemy);

            float total = attackController.GetSequenceDuration(timings);
            Invoke(nameof(EndPlayerTurn), total);
        }
        finally
        {
            Destroy(go);
        }

        CloseCardUI();
    }

    void EndPlayerTurn()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.EndPlayerTurn();
    }

    static Type FindTypeByName(string typeName)
    {
        var asm = typeof(CardController).Assembly;
        return asm.GetTypes()
                  .FirstOrDefault(t => t.Name == typeName && typeof(MonoBehaviour).IsAssignableFrom(t));
    }
}
