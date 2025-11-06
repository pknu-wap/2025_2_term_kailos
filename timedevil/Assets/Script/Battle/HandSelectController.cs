using UnityEngine;
using UnityEngine.UI;

public class HandSelectController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BattleMenuController menu;
    [SerializeField] private HandUI hand;
    [SerializeField] private Image externalSelector; // 옵션
    [SerializeField] private CardUseOrchestrator orchestrator;

    [Header("Behavior")]
    [SerializeField] private bool wrap = true; // (현재 HandUI가 래핑 처리)

    void Reset()
    {
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!hand) hand = FindObjectOfType<HandUI>(true);
        if (!orchestrator) orchestrator = FindObjectOfType<CardUseOrchestrator>(true);
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

        // 메뉴가 Card일 때 E로 선택모드 진입
        if (!hand.IsInSelectMode && menu.Index == 0 && Input.GetKeyDown(KeyCode.E))
        {
            hand.EnterSelectMode();
            menu.EnableInput(false);
            return;
        }

        if (!hand.IsInSelectMode) return;

        // 이동은 HandUI API
        if (Input.GetKeyDown(KeyCode.RightArrow)) hand.MoveSelect(+1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) hand.MoveSelect(-1);

        // 취소(Q): 선택모드 종료
        if (Input.GetKeyDown(KeyCode.Q))
        {
            hand.ExitSelectMode();
            menu.EnableInput(true);
        }

        // 확정(E): 오케스트레이터에 위임
        if (Input.GetKeyDown(KeyCode.E))
            orchestrator?.UseCurrentSelected();
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
    }
}
