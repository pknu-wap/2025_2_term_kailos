// Assets/Script/Battle/DescriptionPanelController.cs
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
    [TextArea] public string msgEnemyTurn = "상대턴입니다."; // ▶ 추가

    [Header("Optional Refs")]
    [SerializeField] private CanvasGroup handCanvasGroup;
    [SerializeField] private bool clearOnAwake = true;
    [SerializeField] private bool logDebug = false;

    private int _lastIndex = -1;
    private bool _forceEnemyTurn = false; // ▶ TurnManager가 켜고 끈다

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

    // ▶ TurnManager가 EnemyTurn 시작/종료 때 호출
    public void SetEnemyTurn(bool on)
    {
        _forceEnemyTurn = on;
        // 적턴이면 숨기고, 아니면 현재 메뉴 인덱스 기준으로 다시 토글
        if (hand != null)
        {
            if (on) hand.HideCards();
            else
            {
                int idx = menu ? menu.Index : 0;
                if (idx == 0) hand.ShowCards(); else hand.HideCards();
            }
        }
        RefreshNow();
    }

    private void RefreshNow()
    {
        if (!descriptionText) return;

        // ▶ 적턴 고정 오버레이
        if (_forceEnemyTurn)
        {
            if (handCanvasGroup)
            {
                handCanvasGroup.alpha = 0f;
                handCanvasGroup.interactable = false;
                handCanvasGroup.blocksRaycasts = false;
            }
            // ⬇⬇⬇ 추가: HandUI 쪽도 확실히 숨김
            if (hand != null) hand.HideCards();

            descriptionText.text = msgEnemyTurn;
            return;
        }

        int index = menu ? menu.Index : 0;

        if (handCanvasGroup)
        {
            bool showHand = (index == 0); // 0=Card일 때만 손패 표시
            handCanvasGroup.alpha = showHand ? 1f : 0f;
            handCanvasGroup.interactable = showHand;
            handCanvasGroup.blocksRaycasts = showHand;
        }

        // ⬇⬇⬇ 추가: HandUI 오브젝트 자체도 토글(시각/입력 모두 일치)
        if (hand != null)
        {
            if (index == 0) hand.ShowCards();
            else hand.HideCards();
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
