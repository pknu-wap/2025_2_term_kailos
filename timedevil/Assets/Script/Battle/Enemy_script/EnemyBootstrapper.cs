using UnityEngine;

[DefaultExecutionOrder(-100)]  // Loader(-500)보다 "뒤"에, 하지만 다른 일반 스크립트보다 "앞"에
public class EnemyBootstrapper : MonoBehaviour
{
    [Header("Database & Selection")]
    [SerializeField] private EnemyDatabaseSO database;
    [SerializeField] private string enemyIdOverride = ""; // 반드시 빈 문자열이어야 Selected 사용

    [Header("Runtime Target")]
    [SerializeField] private EnemyRuntime runtimePrefab;
    private EnemyRuntime runtime;

    void Awake()
    {
        runtime = EnemyRuntime.Instance;
        if (!runtime)
        {
            runtime = FindObjectOfType<EnemyRuntime>();
            if (!runtime && runtimePrefab) runtime = Instantiate(runtimePrefab);
        }
        if (!runtime)
        {
            var go = new GameObject("EnemyRuntime");
            runtime = go.AddComponent<EnemyRuntime>();
        }
    }

    void Start()
    {
        if (!database)
        {
            Debug.LogError("[EnemyBootstrapper] EnemyDatabaseSO is missing.");
            return;
        }

        var selected = SelectedEnemyRuntime.Instance ? SelectedEnemyRuntime.Instance.enemyName : null;
        Debug.Log($"[EnemyBootstrapper] override='{enemyIdOverride}' | selected='{selected}'");

        // 선택 규칙: override가 채워져 있으면 그것 우선
        string chosenId = !string.IsNullOrWhiteSpace(enemyIdOverride) ? enemyIdOverride : selected;

        if (string.IsNullOrWhiteSpace(chosenId))
        {
            if (database.enemies.Count > 0) chosenId = database.enemies[0].enemyId;
            Debug.LogWarning($"[EnemyBootstrapper] No id available. Fallback='{chosenId}'");
        }

        var so = database.GetById(chosenId);
        if (!so)
        {
            Debug.LogError($"[EnemyBootstrapper] Enemy id '{chosenId}' not found in DB.");
            return;
        }

        runtime.InitializeFromSO(so);
        Debug.Log($"[EnemyBootstrapper] Applied SO id='{so.enemyId}', display='{so.displayName}'");

        var hp = FindObjectOfType<HPUIBinder>();
        if (hp)
        {
            var pdr = FindObjectOfType<PlayerDataRuntime>();
            if (pdr) hp.BindPlayer(pdr.Data);
            hp.BindEnemyRuntime(runtime);
            hp.Refresh();
        }
    }
}
