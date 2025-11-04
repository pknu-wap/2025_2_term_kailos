// SceneLoadContext.cs
using UnityEngine;

public class SceneLoadContext : MonoBehaviour
{
    public static SceneLoadContext Instance;

    [Header("Next Battle Params")]
    public string pendingEnemyName = "Enemy1";   // 버튼에서 채워서 battle 씬에서 읽음

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
