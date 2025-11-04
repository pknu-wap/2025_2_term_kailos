// BattleBootstrap.cs (SO-first only)
using UnityEngine;

public class BattleBootstrap : MonoBehaviour
{
    [Header("UI Binder")]
    [SerializeField] private HPUIBinder hpUIBinder;

    [Header("Enemy (SO-first)")]
    [SerializeField] private EnemyDatabaseSO enemyDatabase;
    [SerializeField] private string enemyIdOverride = "";     // 비우면 SelectedEnemyRuntime 사용
    [SerializeField] private EnemyRuntime enemyRuntimePrefab; // 없으면 런타임 자동 생성

    private EnemyRuntime enemyRt;

    private void Awake()
    {
        // PlayerDataRuntime 보장(이미 Ensure 스크립트가 있으면 생략 가능)
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
            if (enemyRuntimePrefab != null)
                enemyRt = Instantiate(enemyRuntimePrefab);
            else
                enemyRt = new GameObject("EnemyRuntime").AddComponent<EnemyRuntime>();
        }
    }

    private void Start()
    {
        // 대상 enemyId 결정
        string enemyId = !string.IsNullOrWhiteSpace(enemyIdOverride)
            ? enemyIdOverride
            : (SelectedEnemyRuntime.Instance != null ? SelectedEnemyRuntime.Instance.enemyName : null);

        if (string.IsNullOrWhiteSpace(enemyId))
            enemyId = "Enemy1";

        Debug.Log($"[BattleBootstrap] enemyId='{enemyId}'");

        // SO 경로만 사용
        if (enemyDatabase == null)
        {
            Debug.LogError("[BattleBootstrap] EnemyDatabaseSO is missing.");
            return;
        }

        var so = enemyDatabase.GetById(enemyId);
        if (so == null)
        {
            Debug.LogError($"[BattleBootstrap] Enemy id '{enemyId}' not found in DB.");
            return;
        }

        enemyRt.InitializeFromSO(so);

        // HP UI 바인딩
        if (hpUIBinder != null)
        {
            var pdr = PlayerDataRuntime.Instance ?? FindObjectOfType<PlayerDataRuntime>(true);
            if (pdr != null) hpUIBinder.BindPlayer(pdr.Data);

            hpUIBinder.BindEnemyRuntime(enemyRt);
            hpUIBinder.Refresh();
        }

        Debug.Log($"[BattleBootstrap] Initialized from EnemySO: {so.displayName} ({so.enemyId})");
    }
}
