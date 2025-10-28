using UnityEngine;
using UnityEngine.UI;

public class BattleMenuController : MonoBehaviour
{
    [Header("Order: 0=Card, 1=Item, 2=Run")]
    [SerializeField] private GameObject[] entries;   // CardPanel, ItemPanel, RunPanel

    [Header("Highlight")]
    [SerializeField] private Color selectedColor = new Color(0.7f, 1f, 0.7f, 1f);
    [SerializeField] private Color normalColor = Color.white;

    // (추후 선/후공 로직 붙일 때 켜고 끌 수 있게)
    [SerializeField] private bool inputEnabled = true;

    private int index = 0;
    private Image[] images;

    void Awake()
    {
        // 패널들에 붙은 Image 캐시
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
            Debug.Log($"[BattleMenu] E pressed → {name} selected (index={index})");
            // TODO: 나중에 여기서 실제 동작(카드 핸드 열기/아이템 창/도주 확인 등) 호출
        }
    }

    public void EnableInput(bool on)
    {
        inputEnabled = on;
        // 입력이 막혔을 때도 하이라이트는 유지(원하면 여기서 모두 normal로 바꿔도 됨)
    }

    private void SetSelection(int newIndex)
    {
        index = Mathf.Clamp(newIndex, 0, entries.Length - 1);

        for (int i = 0; i < entries.Length; i++)
        {
            if (images[i] == null) continue;
            images[i].color = (i == index) ? selectedColor : normalColor;
        }

        // 선택된 패널을 약간 키우고 싶다면(선택사항):
        // for (int i = 0; i < entries.Length; i++)
        // {
        //     if (entries[i] == null) continue;
        //     entries[i].transform.localScale = (i == index) ? Vector3.one * 1.03f : Vector3.one;
        // }
    }
}
