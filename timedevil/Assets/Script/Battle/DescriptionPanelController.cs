// DescriptionPanelController.cs
using TMPro;
using UnityEngine;

public class DescriptionPanelController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private BattleMenuController menu; // �� �ν����Ϳ� ����

    [Header("Messages")]
    [SerializeField] private string msgCard = "Card�� �����մϴ�.";
    [SerializeField] private string msgItem = "Item�� �����մϴ�.";
    [SerializeField] private string msgRun = "����Ĩ�ϴ�.";

    [Header("Options")]
    [SerializeField] private bool clearOnAwake = true;

    void Awake()
    {
        if (clearOnAwake && descriptionText) descriptionText.text = "";
    }

    void OnEnable()
    {
        if (menu != null)
            menu.OnMenuFocusChanged += HandleFocusChanged;
    }

    void OnDisable()
    {
        if (menu != null)
            menu.OnMenuFocusChanged -= HandleFocusChanged;
    }

    private void HandleFocusChanged(int index)
    {
        if (descriptionText == null) return;

        switch (index)
        {
            case 0: descriptionText.text = msgCard; break;
            case 1: descriptionText.text = msgItem; break;
            case 2: descriptionText.text = msgRun; break;
            default: descriptionText.text = ""; break;
        }
        //Debug.Log($"[Desc] focus={index} -> {descriptionText.text}");
    }
}
