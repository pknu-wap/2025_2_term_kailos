using TMPro;
using UnityEngine;

public class DescriptionPanelController : MonoBehaviour
{
    [Header("Target UI")]
    [SerializeField] private TMP_Text descriptionText;

    [Header("Sources")]
    [SerializeField] private BattleMenuController menu;
    [SerializeField] private HandUI hand;
    [SerializeField] private CardDatabaseSO database;

    [Header("Messages")]
    [TextArea] public string msgCard = "Card를 선택합니다.";
    [TextArea] public string msgItem = "Item을 선택합니다.";
    [TextArea] public string msgEnd = "턴엔드합니다.";
    [TextArea] public string msgRun = "도망칩니다.";

    [Header("Optional Refs")]
    [SerializeField] private CanvasGroup handCanvasGroup;
    [SerializeField] private bool clearOnAwake = true;
    [SerializeField] private bool logDebug = false;

    private int _lastIndex = -1;

    void Reset()
    {
        if (!descriptionText) descriptionText = GetComponentInChildren<TMP_Text>();
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!hand) hand = FindObjectOfType<HandUI>(true);
    }

    void Awake()
    {
        if (!descriptionText) descriptionText = GetComponentInChildren<TMP_Text>();
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!hand) hand = FindObjectOfType<HandUI>(true);
    }

    void OnEnable()
    {
        if (menu) menu.onFocusChanged.AddListener(OnMenuFocusChanged);
        if (hand != null)
        {
            hand.onSelectModeChanged += _ => RefreshNow();
            hand.onSelectIndexChanged += _ => RefreshNow();
        }
        _lastIndex = menu ? menu.Index : 0;
        RefreshNow();
    }

    void OnDisable()
    {
        if (menu) menu.onFocusChanged.RemoveListener(OnMenuFocusChanged);
        if (hand != null)
        {
            hand.onSelectModeChanged -= _ => RefreshNow();
            hand.onSelectIndexChanged -= _ => RefreshNow();
        }
    }

    void Start()
    {
        if (clearOnAwake && descriptionText) descriptionText.text = string.Empty;
    }

    void Update()
    {
        if (menu && menu.Index != _lastIndex)
        {
            _lastIndex = menu.Index;
            RefreshNow();
        }
    }

    private void OnMenuFocusChanged(int idx)
    {
        _lastIndex = idx;
        RefreshNow();
    }

    private void RefreshNow()
    {
        if (!descriptionText) return;

        int index = menu ? menu.Index : 0;

        if (handCanvasGroup)
        {
            bool showHand = (index != 3);
            handCanvasGroup.alpha = showHand ? 1f : 0f;
            handCanvasGroup.interactable = showHand;
            handCanvasGroup.blocksRaycasts = showHand;
        }

        // 메뉴가 Card이고 선택모드면 현재 선택 카드의 SO 설명
        if (index == 0 && hand != null && hand.IsInSelectMode)
        {
            string msg = GetCurrentCardDisplay() ?? msgCard;
            descriptionText.text = msg;
            if (logDebug) Debug.Log($"[DescPanel] selecting idx={hand.CurrentSelectIndex}, msg={msg}");
            return;
        }

        // 기본 메시지
        descriptionText.text = index switch
        {
            0 => msgCard,
            1 => msgItem,
            2 => msgEnd,
            3 => msgRun,
            _ => string.Empty
        };
    }

    private string GetCurrentCardDisplay()
    {
        if (database == null || hand == null) return null;

        var ids = hand.VisibleHandIds; // HandUI 스냅샷(화면 순서 보장)
        if (ids == null || ids.Count == 0) return null;

        int i = Mathf.Clamp(hand.CurrentSelectIndex, 0, ids.Count - 1);
        string id = ids[i];

        var so = database.GetById(id);
        if (!so)
        {
            if (logDebug) Debug.LogWarning($"[DescPanel] DB miss for id={id}");
            return $"(등록되지 않은 카드: {id})";
        }
        return so.display;
    }
}
