// TurnManager.cs
using System.Reflection;
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
    private PlayerDataRuntime pdr;

    // ✅ 새 경로
    private EnemyRuntime enemyRt;

    // ⚠️ 구경로(폴백)
    private EnemyDataManager edm;
    private MonoBehaviour enemyComp;

    private int playerSPD = 0;
    private int enemySPD = 0;

    public TurnState currentTurn { get; private set; } = TurnState.PlayerTurn;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!menu) menu = FindObjectOfType<BattleMenuController>();
    }

    void Start()
    {
        pdr = FindObjectOfType<PlayerDataRuntime>();

        // 새 경로
        enemyRt = EnemyRuntime.Instance ?? FindObjectOfType<EnemyRuntime>();

        // 폴백 경로
        edm = EnemyDataManager.Instance ?? FindObjectOfType<EnemyDataManager>();
        if (edm && edm.CurrentEnemyComponent) enemyComp = edm.CurrentEnemyComponent;

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
        // 1) EnemyRuntime 우선
        if (enemyRt)
        {
            enemySPD = Mathf.Max(0, enemyRt.speed);
            return;
        }

        // 2) 폴백: EnemyDataManager 스냅샷
        if (edm && edm.snapshot.speed > 0)
        {
            enemySPD = edm.snapshot.speed;
            return;
        }

        // 3) 최후 폴백: 리플렉션
        if (enemyComp)
        {
            var f = enemyComp.GetType().GetField("speed", BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
            {
                try { enemySPD = Mathf.Max(0, (int)f.GetValue(enemyComp)); }
                catch { enemySPD = 0; }
            }
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
