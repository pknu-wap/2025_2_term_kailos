using TMPro;
using UnityEngine;

public class DescriptionPanelController : MonoBehaviour
{
    [Header("Target UI")]
    [SerializeField] private TMP_Text descriptionText;

    [Header("Sources")]
    [SerializeField] private BattleMenuController menu;    // 0=Card, 1=Item, 2=End, 3=Run

    [Header("Messages")]
    [TextArea] public string msgCard = "Card�� �����մϴ�.";
    [TextArea] public string msgItem = "Item�� �����մϴ�.";
    [TextArea] public string msgEnd = "�Ͽ����մϴ�.";
    [TextArea] public string msgRun = "����Ĩ�ϴ�.";

    [Header("Optional Refs")]
    [SerializeField] private CanvasGroup handCanvasGroup;  // Run(3)�� ���� ����
    [SerializeField] private bool clearOnAwake = true;
    [SerializeField] private bool logDebug = false;

    private int _lastIndex = -1;

    void Reset()
    {
        if (!descriptionText) descriptionText = GetComponentInChildren<TMP_Text>();
        if (!menu) menu = FindObjectOfType<BattleMenuController>();
    }

    void Awake()
    {
        if (!descriptionText) descriptionText = GetComponentInChildren<TMP_Text>();
        if (!menu) menu = FindObjectOfType<BattleMenuController>();
    }

    void OnEnable()
    {
        if (menu) menu.onFocusChanged.AddListener(OnFocusChanged);

        int cur = menu ? menu.Index : 0;
        Apply(cur);
        _lastIndex = cur;
    }

    void OnDisable()
    {
        if (menu) menu.onFocusChanged.RemoveListener(OnFocusChanged);
    }

    void Start()
    {
        if (clearOnAwake && descriptionText) descriptionText.text = string.Empty;
    }

    void Update()
    {
        if (menu && menu.Index != _lastIndex)
        {
            Apply(menu.Index);
            _lastIndex = menu.Index;
        }
    }

    private void OnFocusChanged(int index)
    {
        Apply(index);
        _lastIndex = index;
    }

    private void Apply(int index)
    {
        // 0=Card, 1=Item, 2=End, 3=Run
        string msg = index switch
        {
            0 => msgCard,
            1 => msgItem,
            2 => msgEnd,
            3 => msgRun,
            _ => string.Empty
        };

        if (descriptionText) descriptionText.text = msg;

        // Run(3)�� ���� Hand �г� ����
        if (handCanvasGroup)
        {
            bool showHand = (index != 3);
            handCanvasGroup.alpha = showHand ? 1f : 0f;
            handCanvasGroup.interactable = showHand;
            handCanvasGroup.blocksRaycasts = showHand;
        }

        if (logDebug) Debug.Log($"[DescPanel] focus={index} �� \"{msg}\"");
    }
}
