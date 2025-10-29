using System.Reflection;
using UnityEngine;

public enum TurnState { PlayerTurn, EnemyTurn }

/// <summary>
/// - PlayerDataRuntime / EnemyDataManager에서 SPD를 주입받아 선턴 결정
/// - 현재는 적 턴은 로그만 남기고 즉시 내 턴 복귀(스텁)
/// - 메뉴 입력 연동은 선택(있으면 활성/비활성만 토글)
/// </summary>
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [Header("Optional UI Controller")]
    [SerializeField] private BattleMenuController menu; // 없으면 자동 탐색

    [Header("Delays")]
    [SerializeField] private float enemyThinkDelay = 0.6f;

    // 런타임 소스
    private PlayerDataRuntime pdr;
    private EnemyDataManager edm;

    // 비교용
    private int playerSPD = 0;
    private int enemySPD = 0;

    // 참고용
    private MonoBehaviour enemyComp;

    public TurnState currentTurn { get; private set; } = TurnState.PlayerTurn;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!menu) menu = FindObjectOfType<BattleMenuController>();
    }

    void Start()
    {
        // 주입
        pdr = FindObjectOfType<PlayerDataRuntime>();
        edm = EnemyDataManager.Instance ?? FindObjectOfType<EnemyDataManager>();

        ResolvePlayerData();
        ResolveEnemyData();

        DecideFirstTurn();
    }

    // ---------------- Resolve & Decide ----------------

    void ResolvePlayerData()
    {
        if (pdr != null && pdr.Data != null)
        {
            playerSPD = Mathf.Max(0, pdr.Data.speed);
        }
        else
        {
            playerSPD = 0;
            Debug.LogWarning("[TurnManager] PlayerDataRuntime 또는 Data가 없습니다. SPD=0으로 처리.");
        }
    }

    /// <summary>
    /// 안정화된 적 데이터 해석:
    /// 1) EnemyDataManager가 있으면 snapshot.speed 우선
    /// 2) 없거나 0이면 컴포넌트에서 public int speed 리플렉션
    /// </summary>
    void ResolveEnemyData()
    {
        // 1) 컴포넌트 확보
        if (edm != null && edm.CurrentEnemyComponent != null)
        {
            enemyComp = edm.CurrentEnemyComponent;
        }
        else if (edm != null && edm.currentEnemyComp != null) // (안전망: 필드 직접 접근)
        {
            enemyComp = edm.currentEnemyComp;
        }
        else
        {
            // 마지막 안전망: 씬에서 임의로 찾아보기(테스트용)
            enemyComp = FindObjectOfType<MonoBehaviour>(includeInactive: true);
        }

        // 2) SPD
        if (edm != null && edm.snapshot.speed > 0)
        {
            enemySPD = edm.snapshot.speed;
            Debug.Log($"[TurnManager] Enemy SPD from EnemyDataManager.snapshot = {enemySPD}");
        }
        else
        {
            enemySPD = GetEnemySpeed(enemyComp);
            Debug.Log($"[TurnManager] Enemy SPD via reflection = {enemySPD}");
        }
    }

    int GetEnemySpeed(MonoBehaviour comp)
    {
        if (!comp) return 0;
        var f = comp.GetType().GetField("speed", BindingFlags.Public | BindingFlags.Instance);
        if (f != null)
        {
            try { return Mathf.Max(0, (int)f.GetValue(comp)); }
            catch { /* ignore */ }
        }
        return 0;
    }

    void DecideFirstTurn()
    {
        Debug.Log($"[TurnManager] SPD Compare => Player:{playerSPD} vs Enemy:{enemySPD}");

        if (enemySPD > playerSPD)
        {
            BeginEnemyTurn();
        }
        else
        {
            // 동속이거나 플레이어가 더 빠르면 플레이어 선턴
            BeginPlayerTurn();
        }
    }

    // ---------------- Turn Flow ----------------

    public void BeginPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        if (menu) menu.EnableInput(true);

        Debug.Log("🔷 플레이어 턴 시작");
        // 여기서 필요시: 액션 카운터 리셋, Hand 표시 등 추후 연결
    }

    public void BeginEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        if (menu) menu.EnableInput(false);

        Debug.Log("🔶 적 턴입니다!");
        // 지금은 스텁: 잠깐 대기 후 바로 플레이어 턴 복귀
        StartCoroutine(Co_EnemyTurnStub());
    }

    System.Collections.IEnumerator Co_EnemyTurnStub()
    {
        if (enemyThinkDelay > 0f) yield return new WaitForSeconds(enemyThinkDelay);
        Debug.Log("🔶 적 턴 종료(스텁) → 플레이어 턴으로 복귀");
        BeginPlayerTurn();
    }

    /// <summary>
    /// 플레이어가 ‘Card/Item/Run’ 중 하나를 ‘E’로 확정했을 때 호출해도 되는 훅.
    /// (현재는 즉시 적 턴으로 넘겼다가, 스텁에서 다시 내 턴으로 복귀)
    /// </summary>
    public void OnPlayerActionCommitted()
    {
        if (currentTurn != TurnState.PlayerTurn) return;
        Debug.Log("[TurnManager] Player action committed → EnemyTurn으로 전환");
        BeginEnemyTurn();
    }
}
