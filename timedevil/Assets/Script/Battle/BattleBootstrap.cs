// Assets/Script/Battle/BattleBootstrap.cs
using UnityEngine;

public class BattleBootstrap : MonoBehaviour
{
    [Header("UI Binder")]
    [SerializeField] private HPUIBinder hpUIBinder;

    [Header("Enemy (SO-first)")]
    [SerializeField] private EnemyDatabaseSO enemyDatabase;
    [SerializeField] private string enemyIdOverride = "";     // 비우면 SceneLoadContext 사용
    [SerializeField] private EnemyRuntime enemyRuntimePrefab; // 없으면 런타임 자동 생성

    // (레거시 경로 제거: EnemyFactory/EnemyDataManager 미사용)

    private EnemyRuntime enemyRt;

    private void Awake()
    {
        // PlayerDataRuntime이 없으면 생성
        if (PlayerDataRuntime.Instance == null)
        {
            var go = new GameObject("PlayerDataRuntime (Auto)");
            go.AddComponent<PlayerDataRuntime>();
            Debug.Log("[BattleBootstrap] Auto-created PlayerDataRuntime in battle scene.");
        }

        // EnemyRuntime 확보/생성
        enemyRt = EnemyRuntime.Instance ?? FindObjectOfType<EnemyRuntime>();
        if (enemyRt == null)
        {
            if (enemyRuntimePrefab != null) enemyRt = Instantiate(enemyRuntimePrefab);
            else enemyRt = new GameObject("EnemyRuntime").AddComponent<EnemyRuntime>();
        }
    }

    private void Start()
    {
        // --- 적 ID 결정: Override > SceneLoadContext > 기본값 "Enemy1" ---
        string enemyId =
            !string.IsNullOrWhiteSpace(enemyIdOverride) ? enemyIdOverride :
            (SceneLoadContext.Instance != null && !string.IsNullOrWhiteSpace(SceneLoadContext.Instance.pendingEnemyName))
                ? SceneLoadContext.Instance.pendingEnemyName :
            "Enemy1";

        Debug.Log($"[BattleBootstrap] enemyId (resolved) = '{enemyId}'");

        // DB 디버그(선택)
        if (enemyDatabase)
            Debug.Log($"[BattleBootstrap] DB assigned: count={enemyDatabase.enemies?.Count ?? 0}");

        // 1) SO 우선 경로
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

        // 2) EnemyRuntime 초기화
        enemyRt.InitializeFromSO(so);
        Debug.Log($"[BattleBootstrap] Initialized from EnemySO: {so.displayName} ({so.enemyId})");

        // 3) HP UI 바인딩
        if (hpUIBinder != null)
        {
            var pdr = PlayerDataRuntime.Instance ?? FindObjectOfType<PlayerDataRuntime>();
            if (pdr != null) hpUIBinder.BindPlayer(pdr.Data);

            hpUIBinder.BindEnemyRuntime(enemyRt);
            hpUIBinder.Refresh();
        }
        else
        {
            Debug.LogWarning("[BattleBootstrap] HPUIBinder is not assigned.");
        }
    }
}
