using TMPro;
using UnityEngine;

public class DescriptionPanelController : MonoBehaviour
{
    [Header("Target UI")]
    [SerializeField] private TMP_Text descriptionText;     // ������ ��� TMP_Text

    [Header("Sources")]
    [SerializeField] private BattleMenuController menu;    // ��Ŀ�� �ҽ� (��������� �ڵ� Ž��)

    [Header("Messages")]
    [TextArea] public string msgCard = "Card�� �����մϴ�.";
    [TextArea] public string msgItem = "Item�� �����մϴ�.";
    [TextArea] public string msgRun = "����Ĩ�ϴ�.";

    [Header("Optional Refs")]
    [SerializeField] private CanvasGroup handCanvasGroup;  // Hand �г��� Run������ ����� ���� ��
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
        // �̺�Ʈ ��� ���� (������)
        if (menu)
            menu.onFocusChanged.AddListener(OnFocusChanged);

        // ���� ��Ŀ���� 1ȸ �ݿ�
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
        // ����: �̺�Ʈ�� ����� �� ������ �ε��� ��ȭ�� ������ ����
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

        // Run ��Ŀ���� ���� Hand ����
        if (handCanvasGroup)
        {
            bool showHand = (index != 2);
            handCanvasGroup.alpha = showHand ? 1f : 0f;
            handCanvasGroup.interactable = showHand;
            handCanvasGroup.blocksRaycasts = showHand;
        }

        if (logDebug)
            Debug.Log($"[DescPanel] focus={index} �� \"{msg}\"");
    }
}
