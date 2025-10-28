// SelectedEnemyRuntime.cs
using UnityEngine;

public class SelectedEnemyRuntime : MonoBehaviour
{
    public static SelectedEnemyRuntime Instance { get; private set; }

    [Tooltip("�� ��ȯ �� �Ѱ� ���� �� �̸� (��: Enemy1)")]
    public string enemyName = "Enemy1";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetEnemyName(string name)
    {
        if (!string.IsNullOrEmpty(name)) enemyName = name;
    }
}
