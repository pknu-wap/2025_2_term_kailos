// BattleBootstrap.cs (교체)
using UnityEngine;

public class BattleBootstrap : MonoBehaviour
{
    [Header("UI Binder")]
    [SerializeField] private HPUIBinder hpUIBinder;

    [Header("Enemy (SO-first)")]
    [SerializeField] private EnemyDatabaseSO enemyDatabase;
    [SerializeField] private string enemyIdOverride = "";     // 비우면 SelectedEnemyRuntime 사용
    [SerializeField] private EnemyRuntime enemyRuntimePrefab; // 없으면 런타임 자동 생성

    [Header("Fallback (legacy component attach)")]
    [SerializeField] private GameObject enemyHost; // hierarchy의 'none' 오브젝트(폴백용)

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
        enemyRt = EnemyRuntime.Instance ?? FindObjectOfType<EnemyRuntime>();
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

        // 1) SO 경로 우선
        bool initialized = false;
        if (enemyDatabase != null)
        {
            var so = enemyDatabase.GetById(enemyId);
            if (so != null)
            {
                enemyRt.InitializeFromSO(so);
                initialized = true;

                // HP UI 바인딩
                if (hpUIBinder != null)
                {
                    var pdr = PlayerDataRuntime.Instance ?? FindObjectOfType<PlayerDataRuntime>();
                    if (pdr != null) hpUIBinder.BindPlayer(pdr.Data);
                    hpUIBinder.BindEnemyRuntime(enemyRt);
                    hpUIBinder.Refresh();
                }

                Debug.Log($"[BattleBootstrap] Initialized from EnemySO: {so.displayName} ({so.enemyId})");
            }
        }

        // 2) 폴백: 옛 EnemyFactory + HPUIBinder.BindEnemy
        if (!initialized)
        {
            Debug.LogWarning("[BattleBootstrap] Enemy SO not found or DB missing. Fallback to legacy component path.");

            if (enemyHost == null)
            {
                var found = GameObject.Find("none");
                if (found != null) enemyHost = found;
            }

            if (enemyHost == null)
            {
                Debug.LogError("[BattleBootstrap] enemyHost(null). 'none' 같은 빈 오브젝트를 배치/지정하세요.");
                return;
            }

            var enemyComp = EnemyFactory.AttachEnemyByName(enemyHost, enemyId);
            if (hpUIBinder != null)
            {
                hpUIBinder.BindEnemy(enemyComp);
                hpUIBinder.Refresh();
            }
        }
    }
}
