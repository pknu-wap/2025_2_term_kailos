using UnityEngine;
using UnityEngine.Events;

public class BattleMenuController : MonoBehaviour
{
    [System.Serializable] public class IntEvent : UnityEvent<int> { }

    [Header("Order (2x2 grid): 0=Card, 1=Item / 2=End, 3=Run")]
    [SerializeField] private GameObject[] entries;

    [Header("Input")]
    [SerializeField] private bool inputEnabled = true;

    [Header("Events")]
    public IntEvent onFocusChanged = new IntEvent();
    public IntEvent onSubmit = new IntEvent();   // ✅ 추가: E 키 제출 이벤트

    private int index = 0;
    public int Index => index;
    public int CurrentIndex => index;

    void Start()
    {
        ApplyHighlight(index);
        onFocusChanged?.Invoke(index);
    }

    void Update()
    {
        if (!inputEnabled || entries == null || entries.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.RightArrow)) MoveHoriz(+1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveHoriz(-1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) MoveVert(+1);
        if (Input.GetKeyDown(KeyCode.UpArrow)) MoveVert(-1);

        if (Input.GetKeyDown(KeyCode.E))
        {
            string name = index switch { 0 => "Card", 1 => "Item", 2 => "End", 3 => "Run", _ => $"Idx{index}" };
            Debug.Log($"[BattleMenu] E pressed → {name} selected (index={index})");
            onSubmit?.Invoke(index);   // ✅ 외부로 알림
        }
    }

    public void EnableInput(bool on)
    {
        inputEnabled = on;
        if (on) onFocusChanged?.Invoke(index);
    }

    public void SetFocus(int newIndex)
    {
        newIndex = Mathf.Clamp(newIndex, 0, entries.Length - 1);
        if (newIndex == index) return;

        index = newIndex;
        ApplyHighlight(index);
        onFocusChanged?.Invoke(index);
    }

    private void MoveHoriz(int dir)
    {
        if (entries.Length == 4)
        {
            int row = index / 2;
            int col = index % 2;
            col = (col + (dir > 0 ? 1 : -1) + 2) % 2;
            SetFocus(row * 2 + col);
        }
        else
        {
            int count = entries.Length;
            SetFocus((index + (dir > 0 ? 1 : -1) + count) % count);
        }
    }

    private void MoveVert(int dir)
    {
        if (entries.Length == 4)
        {
            int row = index / 2;
            int col = index % 2;
            row = (row + (dir > 0 ? 1 : -1) + 2) % 2;
            SetFocus(row * 2 + col);
        }
        else
        {
            int count = entries.Length;
            SetFocus((index + (dir > 0 ? 2 : -2) % count + count) % count);
        }
    }

    private void ApplyHighlight(int cur)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (!entries[i]) continue;
            var img = entries[i].GetComponent<UnityEngine.UI.Image>();
            if (!img) continue;
            img.color = (i == cur) ? new Color(0.7f, 1f, 0.7f, 1f) : Color.white;
        }
    }
}
