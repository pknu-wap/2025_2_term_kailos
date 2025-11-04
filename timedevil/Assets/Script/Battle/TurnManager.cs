// Assets/Script/Battle/TurnManager.cs
using UnityEngine;

public enum TurnState { PlayerTurn, EnemyTurn }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [Header("Optional UI Controller")]
    [SerializeField] private BattleMenuController menu;

    [Header("Refs")]
    [SerializeField] private EnemyTurnController enemyTurnController; // 적 턴 실행기
    [SerializeField] private HandUI handUI;                            // 플레이어 카드 UI (적턴에 비활성 처리)
    [SerializeField] private CostController cost;                      // ▶ 플레이어 턴 시작 시 10/10 리셋
    [SerializeField] private DescriptionPanelController desc;          // ▶ 적턴 안내 "상대턴입니다"
    [SerializeField] private BattleDeckRuntime deck;                   // ▶ 플레이어 턴 시작 시 1장 드로우

    [Header("Delays")]
    [SerializeField] private float enemyThinkDelay = 0.6f; // (카운트다운은 EnemyTurnController에서)

    // 런타임 소스
    private PlayerDataRuntime pdr;     // 플레이어 런타임
    private EnemyRuntime enemyRt;      // SO 기반 적 런타임

    private int playerSPD = 0;
    private int enemySPD = 0;

    public TurnState currentTurn { get; private set; } = TurnState.PlayerTurn;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!enemyTurnController) enemyTurnController = FindObjectOfType<EnemyTurnController>(true);
        if (!handUI) handUI = FindObjectOfType<HandUI>(true);
        if (!cost) cost = FindObjectOfType<CostController>(true);
        if (!desc) desc = FindObjectOfType<DescriptionPanelController>(true);
        if (!deck) deck = BattleDeckRuntime.Instance ?? FindObjectOfType<BattleDeckRuntime>(true);
    }

    void Start()
    {
        pdr = FindObjectOfType<PlayerDataRuntime>(true);
        enemyRt = EnemyRuntime.Instance ?? FindObjectOfType<EnemyRuntime>(true);

        ResolvePlayerData();
        ResolveEnemyData();
        DecideFirstTurn();
    }

    void ResolvePlayerData()
    {
        if (pdr && pdr.Data != null) playerSPD = Mathf.Max(0, pdr.Data.speed);
        else { playerSPD = 0; Debug.LogWarning("[TurnManager] PlayerDataRuntime 또는 Data가 없습니다. SPD=0"); }
    }

    void ResolveEnemyData()
    {
        if (enemyRt != null) enemySPD = Mathf.Max(0, enemyRt.speed);
        else { enemySPD = 0; Debug.LogWarning("[TurnManager] EnemyRuntime을 찾지 못했습니다. SPD=0"); }
    }

    void DecideFirstTurn()
    {
        Debug.Log($"[TurnManager] SPD Compare => Player:{playerSPD} vs Enemy:{enemySPD}");
        if (enemySPD > playerSPD) BeginEnemyTurn();
        else BeginPlayerTurn(); // 동속 포함
    }

    public void BeginPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;

        // ▶ 비용 10/10 리셋
        if (cost) cost.ResetTurn();

        // ▶ 플레이어 턴 시작 시 1장 드로우 (가득 찼으면 내부에서 무시)
        if (deck) deck.DrawOneIfNeeded();

        // ▶ 카드 UI ON
        if (handUI) handUI.ShowCards();

        // ▶ 메뉴 입력 ON
        if (menu) menu.EnableInput(true);

        // ▶ 적턴 안내 해제
        if (desc) desc.SetEnemyTurn(false);

        Debug.Log("🔷 플레이어 턴 시작 (드로우 1장 시도)");
    }

    public void BeginEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;

        // ▶ 메뉴 입력 OFF
        if (menu) menu.EnableInput(false);

        // ▶ 카드 UI OFF (적 턴 동안 비활성화)
        if (handUI) handUI.HideCards();

        // ▶ 적턴 안내 표시
        if (desc) desc.SetEnemyTurn(true);

        Debug.Log("🔶 적 턴 시작");

        StartCoroutine(Co_RunEnemyTurnThenBack());
    }

    System.Collections.IEnumerator Co_RunEnemyTurnThenBack()
    {
        if (enemyThinkDelay > 0f) yield return new WaitForSeconds(enemyThinkDelay);

        if (enemyTurnController)
            yield return enemyTurnController.RunTurn();  // 내부에서 5,4,3,2,1 카운트다운

        Debug.Log("🔶 적 턴 종료 → 플레이어 턴");
        BeginPlayerTurn();
    }

    // EndController에서 호출
    public void OnPlayerActionCommitted()
    {
        if (currentTurn != TurnState.PlayerTurn) return;
        Debug.Log("[TurnManager] Player action committed → EnemyTurn");
        BeginEnemyTurn();
    }
}
