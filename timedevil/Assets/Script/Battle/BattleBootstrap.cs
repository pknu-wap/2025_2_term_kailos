// Assets/Script/Battle/BattleBootstrap.cs
using UnityEngine;

public class BattleBootstrap : MonoBehaviour
{
    [Header("UI Binder")]
    [SerializeField] private HPUIBinder hpUIBinder;

    [Header("Enemy (SO-first)")]
    [SerializeField] private EnemyDatabaseSO enemyDatabase;
    [SerializeField] private string enemyIdOverride = "";     // 인스펙터에서 강제 지정 시 사용
    [SerializeField] private EnemyRuntime enemyRuntimePrefab; // 없으면 런타임 자동 생성

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
        if (enemyDatabase == null)
        {
            Debug.LogError("[BattleBootstrap] EnemyDatabaseSO is missing.");
            return;
        }

        // 1) 어떤 적을 쓸지 결정: SceneLoadContext → override → 기본값
        string enemyId = null;

        if (SceneLoadContext.Instance != null && !string.IsNullOrWhiteSpace(SceneLoadContext.Instance.pendingEnemyName))
        {
            enemyId = SceneLoadContext.Instance.pendingEnemyName;
            // 읽었으면 재사용 방지를 위해 비움(원치 않으면 주석 처리)
            SceneLoadContext.Instance.Consume();
        }

        if (string.IsNullOrWhiteSpace(enemyId))
            enemyId = !string.IsNullOrWhiteSpace(enemyIdOverride) ? enemyIdOverride : "Enemy1";

        Debug.Log($"[BattleBootstrap] resolved enemyId='{enemyId}'");

        // 2) DB에서 SO 찾기
        var so = enemyDatabase.GetById(enemyId);
        if (so == null)
        {
            Debug.LogError($"[BattleBootstrap] Enemy id '{enemyId}' not found in DB.");
            return;
        }

        // 3) 런타임 초기화
        enemyRt.InitializeFromSO(so);

        // 4) HP UI 바인딩
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
