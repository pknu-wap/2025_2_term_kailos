// Assets/Script/Battle/BattleBootstrap.cs (교체)
using UnityEngine;

public class BattleBootstrap : MonoBehaviour
{
    [Header("UI Binder")]
    [SerializeField] private HPUIBinder hpUIBinder;

    [Header("Enemy (SO-first)")]
    [SerializeField] private EnemyDatabaseSO enemyDatabase;
    [SerializeField] private string enemyIdOverride = "";     // 필요시만 사용
    [SerializeField] private EnemyRuntime enemyRuntimePrefab; // 없으면 생성

    private EnemyRuntime enemyRt;

    void Awake()
    {
        // PlayerDataRuntime 보장
        if (PlayerDataRuntime.Instance == null)
        {
            var go = new GameObject("PlayerDataRuntime (Auto)");
            go.AddComponent<PlayerDataRuntime>();
            Debug.Log("[BattleBootstrap] Auto-created PlayerDataRuntime in battle scene.");
        }

        // EnemyRuntime 확보
        enemyRt = EnemyRuntime.Instance ?? FindObjectOfType<EnemyRuntime>(true);
        if (enemyRt == null)
        {
            enemyRt = enemyRuntimePrefab != null
                ? Instantiate(enemyRuntimePrefab)
                : new GameObject("EnemyRuntime").AddComponent<EnemyRuntime>();
        }
    }

    void Start()
    {
        if (!enemyDatabase)
        {
            Debug.LogError("[BattleBootstrap] EnemyDatabaseSO is missing.");
            return;
        }

        // ★ 이미 EnemyBootstrapper가 초기화했다면 손대지 않는다
        if (!string.IsNullOrEmpty(enemyRt.enemyId))
        {
            Debug.Log($"[BattleBootstrap] EnemyRuntime already initialized -> '{enemyRt.enemyId}', skip re-init.");
        }
        else
        {
            // (Fallback 경로) 아직 미초기화라면 여기서만 결정
            string chosen =
                  !string.IsNullOrWhiteSpace(enemyIdOverride) ? enemyIdOverride
                : (ObjectNameRuntime.Instance && !string.IsNullOrWhiteSpace(ObjectNameRuntime.Instance.EnemyIDToLoad))
                    ? ObjectNameRuntime.Instance.EnemyIDToLoad
                : (SelectedEnemyRuntime.Instance && !string.IsNullOrWhiteSpace(SelectedEnemyRuntime.Instance.enemyName))
                    ? SelectedEnemyRuntime.Instance.enemyName
                : (enemyDatabase.enemies.Count > 0 ? enemyDatabase.enemies[0].enemyId : null);

            if (string.IsNullOrWhiteSpace(chosen))
            {
                Debug.LogError("[BattleBootstrap] No enemy id could be resolved.");
                return;
            }

            var so = enemyDatabase.GetById(chosen);
            if (!so)
            {
                Debug.LogError($"[BattleBootstrap] Enemy id '{chosen}' not found in DB.");
                return;
            }

            enemyRt.InitializeFromSO(so);
            Debug.Log($"[BattleBootstrap] Initialized from EnemySO: {so.displayName} ({so.enemyId})");
        }

        // UI 바인딩
        var pdr = PlayerDataRuntime.Instance ?? FindObjectOfType<PlayerDataRuntime>(true);
        if (hpUIBinder != null)
        {
            if (pdr) hpUIBinder.BindPlayer(pdr.Data);
            hpUIBinder.BindEnemyRuntime(enemyRt);
            hpUIBinder.Refresh();
        }

        // (선택) 씬 내 HPController들에게 주입
        var allHpControllers = FindObjectsOfType<HPController>(true);
        foreach (var hp in allHpControllers)
            hp.InjectRefs(pdr, enemyRt, hpUIBinder);

        hpUIBinder?.Refresh();
    }
}
