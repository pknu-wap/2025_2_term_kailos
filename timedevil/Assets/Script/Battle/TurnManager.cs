// TurnManager.cs
using UnityEngine;

public enum TurnState { PlayerTurn, EnemyTurn }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [Header("Optional UI Controller")]
    [SerializeField] private BattleMenuController menu;

    [Header("Delays")]
    [SerializeField] private float enemyThinkDelay = 0.6f;

    // 런타임 소스
    private PlayerDataRuntime pdr;   // 플레이어 런타임
    private EnemyRuntime enemyRt;    // SO 기반 적 런타임

    private int playerSPD = 0;
    private int enemySPD = 0;

    public TurnState currentTurn { get; private set; } = TurnState.PlayerTurn;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
    }

    void Start()
    {
        pdr = FindObjectOfType<PlayerDataRuntime>(true);

        // SO 경로
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
        // EnemyRuntime에서만 읽음
        if (enemyRt != null)
        {
            enemySPD = Mathf.Max(0, enemyRt.speed); // EnemyRuntime에 speed 필드/프로퍼티가 있다고 가정
        }
        else
        {
            enemySPD = 0;
            Debug.LogWarning("[TurnManager] EnemyRuntime을 찾지 못했습니다. SPD=0");
        }
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
        if (menu) menu.EnableInput(true);
        Debug.Log("🔷 플레이어 턴 시작");
    }

    public void BeginEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        if (menu) menu.EnableInput(false);
        Debug.Log("🔶 적 턴 시작");
        StartCoroutine(Co_EnemyTurnStub());
    }

    System.Collections.IEnumerator Co_EnemyTurnStub()
    {
        if (enemyThinkDelay > 0f) yield return new WaitForSeconds(enemyThinkDelay);
        Debug.Log("🔶 적 턴 종료(스텁) → 플레이어 턴");
        BeginPlayerTurn();
    }

    public void OnPlayerActionCommitted()
    {
        if (currentTurn != TurnState.PlayerTurn) return;
        Debug.Log("[TurnManager] Player action committed → EnemyTurn");
        BeginEnemyTurn();
    }
}
