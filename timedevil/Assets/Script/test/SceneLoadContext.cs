// Assets/Script/test/SceneLoadContext.cs
using UnityEngine;

public class SceneLoadContext : MonoBehaviour
{
    public static SceneLoadContext Instance { get; private set; }

    [Tooltip("전투 씬으로 전달할 적 ID (예: Enemy1, Enemy2)")]
    public string pendingEnemyName = "";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>값을 읽은 뒤 재사용 방지용으로 비워두고 싶으면 호출</summary>
    public void Consume() => pendingEnemyName = "";
}
