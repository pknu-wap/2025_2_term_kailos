using UnityEngine;
using UnityEngine.Events;

public class BattleMenuController : MonoBehaviour
{
    [System.Serializable] public class IntEvent : UnityEvent<int> { }

    [Header("Order (2x2 grid): 0=Card, 1=Item / 2=End, 3=Run")]
    [SerializeField] private GameObject[] entries;

    [Header("Input")]
    [SerializeField] private bool inputEnabled = true;

    [Header("Events (generic + per entry)")]
    public IntEvent onFocusChanged = new IntEvent();
    public IntEvent onSubmit = new IntEvent();      // 인덱스를 통째로 쏘는 범용 이벤트

    // 엔트리별 이벤트(인스펙터에서 연결해서 쓰기)
    public UnityEvent onCardSubmit = new UnityEvent();
    public UnityEvent onItemSubmit = new UnityEvent();
    public UnityEvent onEndSubmit = new UnityEvent();
    public UnityEvent onRunSubmit = new UnityEvent();

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
            // 공통 제출 이벤트(인덱스 함께)
            onSubmit?.Invoke(index);

            // 엔트리별 이벤트도 함께 발행
            switch (index)
            {
                case 0: onCardSubmit?.Invoke(); break;
                case 1: onItemSubmit?.Invoke(); break;
                case 2: onEndSubmit?.Invoke(); break;   // ← 여기에 EndController.DoEndTurn 연결
                case 3: onRunSubmit?.Invoke(); break;
            }

            Debug.Log($"[BattleMenu] E pressed → selected index={index}");
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
