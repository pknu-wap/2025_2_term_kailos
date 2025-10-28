using UnityEngine;
using UnityEngine.UI;

public class BattleMenuController : MonoBehaviour
{
    [Header("Order: 0=Card, 1=Item, 2=Run")]
    [SerializeField] private GameObject[] entries;   // CardPanel, ItemPanel, RunPanel

    [Header("Highlight")]
    [SerializeField] private Color selectedColor = new Color(0.7f, 1f, 0.7f, 1f);
    [SerializeField] private Color normalColor = Color.white;

    // (���� ��/�İ� ���� ���� �� �Ѱ� �� �� �ְ�)
    [SerializeField] private bool inputEnabled = true;

    private int index = 0;
    private Image[] images;

    void Awake()
    {
        // �гε鿡 ���� Image ĳ��
        images = new Image[entries.Length];
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i] != null)
                images[i] = entries[i].GetComponent<Image>();
        }
    }

    void Start()
    {
        SetSelection(0);
    }

    void Update()
    {
        if (!inputEnabled || entries == null || entries.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SetSelection((index + 1) % entries.Length);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            SetSelection((index - 1 + entries.Length) % entries.Length);

        if (Input.GetKeyDown(KeyCode.E))
        {
            string name = (index == 0) ? "Card" : (index == 1) ? "Item" : "Run";
            Debug.Log($"[BattleMenu] E pressed �� {name} selected (index={index})");
            // TODO: ���߿� ���⼭ ���� ����(ī�� �ڵ� ����/������ â/���� Ȯ�� ��) ȣ��
        }
    }

    public void EnableInput(bool on)
    {
        inputEnabled = on;
        // �Է��� ������ ���� ���̶���Ʈ�� ����(���ϸ� ���⼭ ��� normal�� �ٲ㵵 ��)
    }

    private void SetSelection(int newIndex)
    {
        index = Mathf.Clamp(newIndex, 0, entries.Length - 1);

        for (int i = 0; i < entries.Length; i++)
        {
            if (images[i] == null) continue;
            images[i].color = (i == index) ? selectedColor : normalColor;
        }

        // ���õ� �г��� �ణ Ű��� �ʹٸ�(���û���):
        // for (int i = 0; i < entries.Length; i++)
        // {
        //     if (entries[i] == null) continue;
        //     entries[i].transform.localScale = (i == index) ? Vector3.one * 1.03f : Vector3.one;
        // }
    }
}
