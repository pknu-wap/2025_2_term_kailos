using UnityEngine;

[DefaultExecutionOrder(-500)] // Bootstrapper보다 먼저!
public class BattleEnemyLoader : MonoBehaviour
{
    [SerializeField] private bool alsoWriteToSelectedEnemy = true;

    private void Awake()
    {
        // 1) ObjectNameRuntime에서 적 ID 읽기
        var id = ObjectNameRuntime.Instance ? ObjectNameRuntime.Instance.EnemyIDToLoad : null;

        if (!string.IsNullOrEmpty(id))
        {
            // 2) 선택 싱글톤에도 반영(기존 EnemyBootstrapper 스타트 로직을 그대로 활용)
            if (alsoWriteToSelectedEnemy)
            {
                var sel = SelectedEnemyRuntime.Instance;
                if (!sel)
                {
                    var go = new GameObject("SelectedEnemyRuntime");
                    sel = go.AddComponent<SelectedEnemyRuntime>();
                }
                sel.SetEnemyName(id);
            }
            Debug.Log($"[BattleEnemyLoader] 준비 완료: '{id}' 로드 예정");
        }
        else
        {
            Debug.LogWarning("[BattleEnemyLoader] EnemyIDToLoad가 비어 있음 → DB fallback 사용");
        }
    }
}
