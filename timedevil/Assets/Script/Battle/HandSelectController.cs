using UnityEngine;
using UnityEngine.UI;

public class HandSelectController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BattleMenuController menu; // 포커스/입력 소스
    [SerializeField] private HandUI hand;               // 카드 아이콘을 그리는 UI
    [SerializeField] private Image selector;            // 오렌지 테두리 이미지(활성/위치 이동)

    [Header("Behavior")]
    [SerializeField] private bool wrap = true;          // 경계에서 래핑 이동

    private bool selecting = false;
    private int selIndex = 0;

    void Reset()
    {
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!hand) hand = FindObjectOfType<HandUI>(true);
    }

    void Awake()
    {
        if (selector) selector.enabled = false;
    }

    void Update()
    {
        // 선택 모드가 아닐 때: Card가 포커스된 상태에서 E로 진입
        if (!selecting)
        {
            if (menu && menu.Index == 0 && Input.GetKeyDown(KeyCode.E))
                EnterSelectMode();
            return;
        }

        // === 선택 모드 === (메뉴 입력은 꺼져 있음)
        if (hand == null || hand.CardCount == 0)
        {
            // 손패가 없으면 자동 종료
            ExitSelectMode();
            return;
        }

        // 좌/우 이동
        if (Input.GetKeyDown(KeyCode.RightArrow))
            Move(+1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            Move(-1);

        // Q: 선택 취소(선택 모드 종료)
        if (Input.GetKeyDown(KeyCode.Q))
            ExitSelectMode();

        // E: 확정(여기서는 로그만 남기고 종료; 너 로직에 맞춰 훅 연결해)
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"[HandSelect] Use hand index = {selIndex}");
            // TODO: BattleDeckRuntime.Instance.UseCardToBottom(selIndex) 같은 실제 사용 로직 연결
            ExitSelectMode();
        }
    }

    private void EnterSelectMode()
    {
        if (hand == null || hand.CardCount == 0) return;

        selecting = true;
        selIndex = Mathf.Clamp(selIndex, 0, hand.CardCount - 1);

        // 메뉴 입력 OFF
        if (menu) menu.EnableInput(false);

        // 선택 박스 표시 + 위치 맞추기
        if (selector)
        {
            selector.enabled = true;
            SnapSelectorTo(selIndex);
        }

        Debug.Log("[Bridge] Hand select mode entered.");
    }

    private void ExitSelectMode()
    {
        selecting = false;

        // 선택 박스 숨김
        if (selector) selector.enabled = false;

        // 메뉴 입력 ON
        if (menu) menu.EnableInput(true);

        Debug.Log("[Bridge] Hand select mode exited.");
    }

    private void Move(int delta)
    {
        int count = hand.CardCount;
        if (count <= 0) return;

        int next = selIndex + delta;

        if (wrap)
            next = (next % count + count) % count; // 안전 래핑
        else
            next = Mathf.Clamp(next, 0, count - 1);

        if (next != selIndex)
        {
            selIndex = next;
            SnapSelectorTo(selIndex);
        }
    }

    private void SnapSelectorTo(int index)
    {
        var rt = hand.GetCardRect(index);
        if (!rt || !selector) return;

        // 화면 좌표 기준 정렬(같은 Canvas에 있으면 position으로 OK)
        selector.rectTransform.position = rt.position;

        // 선택 박스 크기를 카드에 맞춰주고 싶다면(선택):
        // selector.rectTransform.sizeDelta = rt.sizeDelta;
    }
}
