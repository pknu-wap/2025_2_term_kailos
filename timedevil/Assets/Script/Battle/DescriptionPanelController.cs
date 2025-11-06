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
    [TextArea] public string msgEnemyTurn = "상대턴입니다."; // 적턴 고정 안내

    [Header("Optional Refs")]
    [SerializeField] private CanvasGroup handCanvasGroup;
    [SerializeField] private bool clearOnAwake = true;
    [SerializeField] private bool logDebug = false;

    private int _lastIndex = -1;
    private bool _forceEnemyTurn = false;   // TurnManager에서 on/off
    private string _forcedMessage = null;   // 👈 발동 중(explanation) 임시 고정 문구

    void Reset()
    {
        if (!descriptionText) descriptionText = GetComponentInChildren<TMP_Text>(true);
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!hand) hand = FindObjectOfType<HandUI>(true);
    }

    void Awake()
    {
        if (!descriptionText) descriptionText = GetComponentInChildren<TMP_Text>(true);
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

    // TurnManager가 EnemyTurn 시작/종료 때 호출
    public void SetEnemyTurn(bool on)
    {
        _forceEnemyTurn = on;

        // 적턴이면 손패 UI 숨김, 아니라면 현재 메뉴 인덱스 기준으로 토글
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

    // 👇 카드 발동(관전 모드) 동안 임시 문구를 고정 표시
    public void ShowTemporaryExplanation(string text)
    {
        _forcedMessage = text;
        if (logDebug) Debug.Log($"[DescPanel] forcedMessage ON: {text}");
        RefreshNow();
    }

    public void ClearTemporaryMessage()
    {
        if (logDebug) Debug.Log("[DescPanel] forcedMessage OFF");
        _forcedMessage = null;
        RefreshNow();
    }

    private void RefreshNow()
    {
        if (!descriptionText) return;

        // ✅ 손패 표시/입력은 적턴이면 항상 끈다 (시각/입력 통일)
        if (_forceEnemyTurn && handCanvasGroup)
        {
            handCanvasGroup.alpha = 0f;
            handCanvasGroup.interactable = false;
            handCanvasGroup.blocksRaycasts = false;
        }
        if (_forceEnemyTurn && hand != null)
            hand.HideCards();

        // ✅ "발동 중 임시 문구"가 있으면 적턴이라도 이것을 최우선으로 보여준다
        if (!string.IsNullOrEmpty(_forcedMessage))
        {
            descriptionText.text = _forcedMessage;
            return;
        }

        // ✅ 임시 문구가 없고, 적턴이면 기본 적턴 문구
        if (_forceEnemyTurn)
        {
            descriptionText.text = msgEnemyTurn;
            return;
        }

        // ↓↓↓ 이하 기존 로직 그대로
        int index = menu ? menu.Index : 0;

        if (handCanvasGroup)
        {
            bool showHand = (index == 0);
            handCanvasGroup.alpha = showHand ? 1f : 0f;
            handCanvasGroup.interactable = showHand;
            handCanvasGroup.blocksRaycasts = showHand;
        }

        if (hand != null)
        {
            if (index == 0) hand.ShowCards();
            else hand.HideCards();
        }

        if (index == 0 && hand != null && hand.IsInSelectMode)
        {
            string msg = GetCurrentCardDisplay() ?? msgCard;
            descriptionText.text = msg;
            return;
        }

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

        var ids = hand.VisibleHandIds; // HandUI 스냅샷
        if (ids == null || ids.Count == 0) return null;

        int i = Mathf.Clamp(hand.CurrentSelectIndex, 0, ids.Count - 1);
        string id = ids[i];

        var so = database.GetById(id);
        if (!so)
        {
            if (logDebug) Debug.LogWarning($"[DescPanel] DB miss for id={id}");
            return $"(등록되지 않은 카드: {id})";
        }
        // 선택 모드에서는 display(설명문) 사용
        return string.IsNullOrEmpty(so.display) ? "(설명이 없습니다)" : so.display;
    }


}
