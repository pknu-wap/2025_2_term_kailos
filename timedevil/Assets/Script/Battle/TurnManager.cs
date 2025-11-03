using UnityEngine;

public enum TurnState { PlayerTurn, EnemyTurn }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [Header("Optional UI Controller")]
    [SerializeField] private BattleMenuController menu;

    [Header("Delays")]
    [SerializeField] private float enemyThinkDelay = 0.0f; // 지금은 즉시

    // 런타임 소스
    private PlayerDataRuntime pdr;
    private EnemyRuntime enemyRt;

    // 적 턴 실행기
    [SerializeField] private EnemyTurnController enemyTurnController;
    [SerializeField] private HandUI handUI;
    [SerializeField] private CostController costController;

    public TurnState currentTurn { get; private set; } = TurnState.PlayerTurn;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!enemyTurnController) enemyTurnController = FindObjectOfType<EnemyTurnController>(true);
        if (!handUI) handUI = FindObjectOfType<HandUI>(true);
        if (!costController) costController = FindObjectOfType<CostController>(true);
    }

    void Start()
    {
        pdr = FindObjectOfType<PlayerDataRuntime>(true);
        enemyRt = EnemyRuntime.Instance ?? FindObjectOfType<EnemyRuntime>(true);
        DecideFirstTurn();
    }

    void DecideFirstTurn()
    {
        // 간단: 플레이어 선(추후 SPD 비교)
        BeginPlayerTurn();
    }

    public void BeginPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;

        // (선택) 플레이어 자원 리셋
        // if (costController) costController.ResetTo(10);

        // 플레이어 손패 바인딩 및 안전 복구
        if (handUI)
        {
            handUI.BindToPlayer();
            handUI.EnableAllCardImages();   // 프리뷰/적턴에서 꺼졌던 이미지 안전 복구
            handUI.RebuildFromHand();
        }

        if (menu) menu.EnableInput(true);
        Debug.Log("🔷 플레이어 턴 시작");
    }

    public void BeginEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        if (menu) menu.EnableInput(false);

        Debug.Log("🔶 적 턴 시작");
        StartCoroutine(Co_RunEnemyTurnThenBack());
    }

    System.Collections.IEnumerator Co_RunEnemyTurnThenBack()
    {
        if (enemyThinkDelay > 0f) yield return new WaitForSeconds(enemyThinkDelay);

        if (enemyTurnController)
            yield return enemyTurnController.RunTurn();

        // 적 턴 끝 → 플레이어 턴 복귀
        Debug.Log("🔶 적 턴 종료 → 플레이어 턴");
        BeginPlayerTurn();
    }

    /// <summary>플레이어가 End를 눌러 턴을 넘길 때 호출</summary>
    public void OnPlayerActionCommitted()
    {
        if (currentTurn != TurnState.PlayerTurn) return;
        BeginEnemyTurn();
    }
}
