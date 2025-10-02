using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum TurnState { PlayerTurn, EnemyTurn }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public TurnState currentTurn;

    [Header("UI Buttons (플레이어 턴 활성/비활성)")]
    public Button cardBtn;
    public Button moveBtn;
    public Button itemBtn;
    public Button runBtn;

    [Header("Enemy Controller")]
    [SerializeField] private EnemyController enemyController;

    [Header("Hand UI")]
    [SerializeField] private BattleHandUI handUI;   // ← 손패 UI 레퍼런스

    [Header("Delays")]
    [Tooltip("적 턴 시작 시 잠깐의 '고민시간'(초)")]
    public float enemyThinkDelay = 0.6f;

    public bool usedCardThisTurn { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // 시작 시 손패 UI는 닫아둔다
        if (handUI) handUI.SetVisible(false);
        StartPlayerTurn();
    }

    // --------------- Player Turn ---------------

    public void StartPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        usedCardThisTurn = false;
        SetButtons(true);

        // 손패 보충/갱신(하지만 UI는 닫아둔다)
        if (handUI)
        {
            handUI.OnPlayerTurnStart();
            handUI.SetVisible(false); // 기본 닫힘, Card 버튼으로 열기
        }

        Debug.Log("🔷 플레이어 턴 시작");
    }

    public bool TryConsumeCardUseThisTurn()
    {
        if (usedCardThisTurn) return false;
        usedCardThisTurn = true;
        return true;
    }

    public void EndPlayerTurn()
    {
        // 엔드 페이즈: 3장 초과분은 덱 밑으로
        var bd = BattleDeckRuntime.Instance;
        if (bd != null)
        {
            while (bd.hand.Count > 3)
                bd.UseCardToBottom(bd.hand.Count - 1);
        }

        // 손패 UI는 턴이 끝나면 반드시 닫는다
        if (handUI)
        {
            handUI.Refresh();      // 반영
            handUI.SetVisible(false);
        }

        SetButtons(false);
        StartCoroutine(Co_EnemyTurn());
    }

    // --------------- Enemy Turn ---------------

    IEnumerator Co_EnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("🔶 적 턴 시작");

        // 혹시 모를 열림 상태 방지
        if (handUI) handUI.SetVisible(false);

        if (enemyThinkDelay > 0f)
            yield return new WaitForSeconds(enemyThinkDelay);

        if (enemyController != null)
            yield return enemyController.ExecuteOneAction();
        else
            Debug.LogWarning("[TurnManager] EnemyController 미연결");

        Debug.Log("🔶 적 턴 종료");
        StartPlayerTurn();
    }

    // --------------- Helpers ---------------

    void SetButtons(bool on)
    {
        if (cardBtn) cardBtn.interactable = on;
        if (moveBtn) moveBtn.interactable = on;
        if (itemBtn) itemBtn.interactable = on;
        if (runBtn) runBtn.interactable = on;
    }
}
