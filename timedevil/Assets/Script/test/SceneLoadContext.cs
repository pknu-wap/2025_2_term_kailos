// Assets/Script/Battle/SceneLoadContext.cs
using UnityEngine;

public class SceneLoadContext : MonoBehaviour
{
    public static SceneLoadContext Instance { get; private set; }

    [Tooltip("마이룸 등 선행 씬에서 전투 씬으로 넘길 적 ID")]
    public string pendingEnemyName = null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
