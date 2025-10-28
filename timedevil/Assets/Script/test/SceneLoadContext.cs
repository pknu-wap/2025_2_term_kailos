// SceneLoadContext.cs
using UnityEngine;

public class SceneLoadContext : MonoBehaviour
{
    public static SceneLoadContext Instance;

    [Header("Next Battle Params")]
    public string pendingEnemyName = "Enemy1";   // ��ư���� ä���� battle ������ ����

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
