// BattleMenuController.cs
using System;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenuController : MonoBehaviour
{
    [Header("Order: 0=Card, 1=Item, 2=Run")]
    [SerializeField] private GameObject[] entries;

    [Header("Highlight")]
    [SerializeField] private Color selectedColor = new Color(0.7f, 1f, 0.7f, 1f);
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private bool inputEnabled = true;

    private int index = 0;
    private Image[] images;

    // ✅ 포커스 변경을 외부에 알려주는 이벤트
    public event Action<int> OnMenuFocusChanged;
    public event Action<int> FocusChanged;
    public int CurrentIndex { get; private set; }

    void Awake()
    {
        images = new Image[entries.Length];
        for (int i = 0; i < entries.Length; i++)
            if (entries[i] != null) images[i] = entries[i].GetComponent<Image>();
    }

    void Start()
    {
        SetSelection(0);
        // ✅ 시작 시에도 한 번 알려주기
        OnMenuFocusChanged?.Invoke(index);
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
        }
    }

    public void EnableInput(bool on) => inputEnabled = on;

    private void SetSelection(int newIndex)
    {
        CurrentIndex = Mathf.Clamp(newIndex, 0, entries.Length - 1);
        index = Mathf.Clamp(newIndex, 0, entries.Length - 1);

        for (int i = 0; i < entries.Length; i++)
            if (images[i] != null)
                images[i].color = (i == index) ? selectedColor : normalColor;

        // ✅ 포커스 바뀔 때마다 알림
        OnMenuFocusChanged?.Invoke(index);
        FocusChanged?.Invoke(CurrentIndex);
    }

}
