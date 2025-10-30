using UnityEngine;
using UnityEngine.Events;

public class BattleMenuController : MonoBehaviour
{
    [System.Serializable] public class IntEvent : UnityEvent<int> { }

    [Header("Order: 0=Card, 1=Item, 2=Run")]
    [SerializeField] private GameObject[] entries;

    [Header("Input")]
    [SerializeField] private bool inputEnabled = true;

    [Header("Events")]
    public IntEvent onFocusChanged = new IntEvent();

    private int index = 0;
    public int Index => index;           // ✅ 다른 스크립트 호환용
    public int CurrentIndex => index;

    void Start()
    {
        // 하이라이트만 적용
        ApplyHighlight(index);

        // ✅ 시작하자마자 현재 포커스를 '동적 int'로 브로드캐스트
        onFocusChanged?.Invoke(index);
    }

    void Update()
    {
        if (!inputEnabled || entries == null || entries.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.RightArrow)) SetFocus((index + 1) % entries.Length);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) SetFocus((index - 1 + entries.Length) % entries.Length);

        if (Input.GetKeyDown(KeyCode.E))
        {
            var name = index == 0 ? "Card" : index == 1 ? "Item" : "Run";
            Debug.Log($"[BattleMenu] E pressed → {name} selected (index={index})");
        }
    }

    public void SetFocus(int newIndex)
    {
        newIndex = Mathf.Clamp(newIndex, 0, entries.Length - 1);
        if (newIndex == index) return;

        index = newIndex;
        ApplyHighlight(index);

        // ✅ 포커스 바뀔 때마다 이벤트 발행
        onFocusChanged?.Invoke(index);
    }

    private void ApplyHighlight(int cur)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (!entries[i]) continue;
            var img = entries[i].GetComponent<UnityEngine.UI.Image>();
            if (!img) continue;

            // 네가 쓰던 색 적용 로직 유지해도 됨
            img.color = (i == cur) ? new Color(0.7f, 1f, 0.7f, 1f) : Color.white;
        }
    }
    public void EnableInput(bool on)
    {
        inputEnabled = on;

        // (옵션) 입력을 켰을 때, 현재 포커스를 다시 통지해서
        // Hand 갱신/설명 패널 반영이 즉시 일어나도록 함.
        if (on)
            onFocusChanged?.Invoke(index);

        // (옵션) 시각적 비활성화가 필요하면 CanvasGroup을 달아 제어하세요.
        // var cg = GetComponent<CanvasGroup>();
        // if (cg) { cg.interactable = on; cg.blocksRaycasts = on; cg.alpha = on ? 1f : 0.7f; }
    }
}
