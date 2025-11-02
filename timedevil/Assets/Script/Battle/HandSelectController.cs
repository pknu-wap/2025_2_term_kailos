using UnityEngine;
using UnityEngine.UI;

public class HandSelectController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BattleMenuController menu;
    [SerializeField] private HandUI hand;
    [SerializeField] private Image externalSelector; // 옵션

    [Header("Behavior")]
    [SerializeField] private bool wrap = true;

    void Reset()
    {
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!hand) hand = FindObjectOfType<HandUI>(true);
    }

    void Awake()
    {
        if (externalSelector) externalSelector.enabled = false;
    }

    void OnEnable()
    {
        if (hand != null)
        {
            hand.onSelectModeChanged += OnHandSelectModeChanged;
            hand.onSelectIndexChanged += OnHandIndexChanged;
        }
    }

    void OnDisable()
    {
        if (hand != null)
        {
            hand.onSelectModeChanged -= OnHandSelectModeChanged;
            hand.onSelectIndexChanged -= OnHandIndexChanged;
        }
    }

    void Update()
    {
        if (!menu || !hand) return;

        // ● 선택모드가 아닐 때: Card(0)에서 E로 진입
        if (!hand.IsInSelectMode && menu.Index == 0 && Input.GetKeyDown(KeyCode.E))
        {
            hand.EnterSelectMode();
            menu.EnableInput(false); // 메뉴 이동 잠금
            return;
        }

        if (!hand.IsInSelectMode) return;

        // ● 선택모드: 이동은 HandUI API만 호출 (중복 방지)
        if (Input.GetKeyDown(KeyCode.RightArrow)) hand.MoveSelect(+1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) hand.MoveSelect(-1);

        // ● 종료: Q (원하신대로 E는 아무 기능 없음)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            hand.ExitSelectMode();
            menu.EnableInput(true); // 메뉴 이동 복구
        }
    }

    private void OnHandSelectModeChanged(bool on)
    {
        if (!externalSelector) return;
        externalSelector.enabled = on;
        if (on) SnapExternalSelector(hand.CurrentSelectIndex);
    }

    private void OnHandIndexChanged(int idx)
    {
        if (!externalSelector) return;
        SnapExternalSelector(idx);
    }

    private void SnapExternalSelector(int index)
    {
        if (!externalSelector) return;
        var rt = hand.GetCardRect(index);
        if (!rt) return;
        externalSelector.rectTransform.position = rt.position;
        // 필요 시 크기 맞추기:
        // externalSelector.rectTransform.sizeDelta = rt.sizeDelta;
    }
}
