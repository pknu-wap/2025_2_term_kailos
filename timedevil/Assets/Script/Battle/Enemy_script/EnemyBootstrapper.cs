using UnityEngine;

public class EnemyBootstrapper : MonoBehaviour
{
    [Header("Database & Selection")]
    [SerializeField] private EnemyDatabaseSO database;
    [SerializeField] private string enemyIdOverride = ""; // 비우면 SelectedEnemyRuntime 사용

    [Header("Runtime Target")]
    [SerializeField] private EnemyRuntime runtimePrefab; // 없으면 자동 생성
    private EnemyRuntime runtime;

    void Awake()
    {
        runtime = EnemyRuntime.Instance ?? FindObjectOfType<EnemyRuntime>();
        if (!runtime)
        {
            if (runtimePrefab) runtime = Instantiate(runtimePrefab);
            else
            {
                var go = new GameObject("EnemyRuntime");
                runtime = go.AddComponent<EnemyRuntime>();
            }
        }
    }

    void Start()
    {
        if (!database)
        {
            Debug.LogError("[EnemyBootstrapper] EnemyDatabaseSO is missing.");
            return;
        }

        string chosenId =
            !string.IsNullOrWhiteSpace(enemyIdOverride) ? enemyIdOverride :
            (SelectedEnemyRuntime.Instance ? SelectedEnemyRuntime.Instance.enemyName : null);

        if (string.IsNullOrWhiteSpace(chosenId))
        {
            Debug.LogWarning("[EnemyBootstrapper] No enemy id given. Use first in DB.");
            if (database.enemies.Count > 0) chosenId = database.enemies[0].enemyId;
        }

        var so = database.GetById(chosenId);
        if (!so)
        {
            Debug.LogError($"[EnemyBootstrapper] Enemy id '{chosenId}' not found in DB.");
            return;
        }

        runtime.InitializeFromSO(so);

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
