using TMPro;
using UnityEngine;

public class DescriptionPanelController : MonoBehaviour
{
    [Header("Target UI")]
    [SerializeField] private TMP_Text descriptionText;     // 설명을 띄울 TMP_Text

    [Header("Sources")]
    [SerializeField] private BattleMenuController menu;    // 포커스 소스 (비어있으면 자동 탐색)

    [Header("Messages")]
    [TextArea] public string msgCard = "Card를 선택합니다.";
    [TextArea] public string msgItem = "Item을 선택합니다.";
    [TextArea] public string msgRun = "도망칩니다.";

    [Header("Optional Refs")]
    [SerializeField] private CanvasGroup handCanvasGroup;  // Hand 패널을 Run에서만 숨기고 싶을 때
    [SerializeField] private bool clearOnAwake = true;
    [SerializeField] private bool logDebug = false;

    private int _lastIndex = -1;

    void Reset()
    {
        if (!descriptionText)
            descriptionText = GetComponentInChildren<TMP_Text>();
        if (!menu)
            menu = FindObjectOfType<BattleMenuController>();
    }

    void Awake()
    {
        if (!descriptionText)
            descriptionText = GetComponentInChildren<TMP_Text>();
        if (!menu)
            menu = FindObjectOfType<BattleMenuController>();
    }

    void OnEnable()
    {
        // 이벤트 기반 구독 (있으면)
        if (menu)
            menu.onFocusChanged.AddListener(OnFocusChanged);

        // 현재 포커스로 1회 반영
        int cur = menu ? menu.Index : 0;
        Apply(cur);
        _lastIndex = cur;
    }

    void OnDisable()
    {
        if (menu)
            menu.onFocusChanged.RemoveListener(OnFocusChanged);
    }

    void Start()
    {
        if (clearOnAwake && descriptionText)
            descriptionText.text = string.Empty;
    }

    void Update()
    {
        // 폴백: 이벤트가 제대로 안 오더라도 인덱스 변화를 감지해 갱신
        if (menu && menu.Index != _lastIndex)
        {
            Apply(menu.Index);
            _lastIndex = menu.Index;
        }
    }

    // ----- handlers -----
    private void OnFocusChanged(int index)
    {
        Apply(index);
        _lastIndex = index;
    }

    private void Apply(int index)
    {
        // 0=Card, 1=Item, 2=Run
        string msg = (index == 0) ? msgCard :
                     (index == 1) ? msgItem : msgRun;

        if (descriptionText)
            descriptionText.text = msg;

        // Run 포커스일 때만 Hand 숨김
        if (handCanvasGroup)
        {
            bool showHand = (index != 2);
            handCanvasGroup.alpha = showHand ? 1f : 0f;
            handCanvasGroup.interactable = showHand;
            handCanvasGroup.blocksRaycasts = showHand;
        }

        if (logDebug)
            Debug.Log($"[DescPanel] focus={index} → \"{msg}\"");
    }
}
