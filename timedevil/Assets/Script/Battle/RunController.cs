using UnityEngine;

public class RunController : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private BattleMenuController menu; // 없으면 자동 탐색
    [SerializeField] private int runIndex = 3;          // 0=Card,1=Item,2=End,3=Run

    [Header("Options")]
    [SerializeField] private float graceSeconds = 1.0f; // 돌아간 뒤 재충돌 방지 시간

    private bool isReturning = false;

    void Awake()
    {
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
    }

    void OnEnable()
    {
        if (menu) menu.onSubmit.AddListener(OnMenuSubmit);
    }

    void OnDisable()
    {
        if (menu) menu.onSubmit.RemoveListener(OnMenuSubmit);
    }

    // 메뉴에서 E키로 확정될 때 들어오는 이벤트
    private void OnMenuSubmit(int idx)
    {
        if (idx == runIndex)
            OnRunPressed();
    }

    // (UI 버튼에서도 직접 연결 가능)
    public void OnRunPressed()
    {
        if (isReturning) return;

        // 플레이어 턴/상태에서만 허용(원하면 빼도 됨)
        var tm = FindObjectOfType<TurnManager>(true);
        if (tm)
        {
            if (tm.currentTurn != TurnState.PlayerTurn) return;
            if (tm.IsPlayerDiscardPhase) return;
        }

        if (string.IsNullOrWhiteSpace(PlayerReturnContext.ReturnSceneName))
        {
            Debug.LogWarning("[RunController] 돌아갈 씬 정보가 없습니다.");
            return;
        }

        isReturning = true;

        // 배틀 입력 잠금(선택)
        var m = menu ? menu : FindObjectOfType<BattleMenuController>(true);
        if (m) m.EnableInput(false);

        // 돌아가기
        SceneLoader.GoBackToReturnScene(graceSeconds, useFaderIfExists: true);
    }
}
