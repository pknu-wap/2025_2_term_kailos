using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// �� �̸�(��: "Enemy1")�� �޾� �ش� Ÿ�� ������Ʈ�� enemyHost�� ���̰�,
/// public �ʵ� �⺻���� �о� ���������� ����.
/// HPUIBinder�͵� ������ UI�� ��� �ݿ�.
/// </summary>
public class EnemyDataManager : MonoBehaviour
{
    public static EnemyDataManager Instance { get; private set; }

    [Header("Where to attach enemy component (ex: 'none' GameObject)")]
    [SerializeField] private Transform enemyHost;

    [Header("Debug / Fallback")]
    [SerializeField] private string enemyNameOverride = "Enemy1";

    [Header("Runtime")]
    public MonoBehaviour currentEnemyComp;   // Enemy1 �� ���� ������Ʈ
    public EnemySnapshot snapshot;           // �о�� �� ����

    /// <summary>
    /// TurnManager ��� �б� ���� �����ϴ� �б� ���� ������Ƽ(��õ).
    /// </summary>
    public MonoBehaviour CurrentEnemyComponent => currentEnemyComp;

    [Serializable]
    public struct EnemySnapshot
    {
        public string enemyName;
        public int maxHP, currentHP, attack, defense, speed;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // 1) �� �̸� ����
        string enemyName = SceneLoadContext.Instance != null
            ? SceneLoadContext.Instance.pendingEnemyName
            : null;
        if (string.IsNullOrWhiteSpace(enemyName))
            enemyName = enemyNameOverride;

        // 2) ȣ��Ʈ�� ������� �̸����� ã�ƺ���
        if (enemyHost == null)
        {
            var found = GameObject.Find("none");
            if (found != null) enemyHost = found.transform;
        }
        if (enemyHost == null)
        {
            Debug.LogError("[EnemyDataManager] enemyHost�� �����ϴ�. 'none' ���� �� ������Ʈ�� �����ϼ���.");
            return;
        }

        // 3) �� ������Ʈ ���� + ������ �б�
        AttachAndRead(enemyName);

        // 4) HPUIBinder ����
        var hp = FindObjectOfType<HPUIBinder>();
        if (hp != null)
        {
            // PlayerDataRuntime���� �÷��̾� ������ ������ ���ε�(������)
            var pdr = FindObjectOfType<PlayerDataRuntime>();
            if (pdr != null) hp.BindPlayer(pdr.Data);

            if (currentEnemyComp != null) hp.BindEnemy(currentEnemyComp);
            hp.Refresh();
        }
        else
        {
            Debug.LogWarning("[EnemyDataManager] HPUIBinder�� ������ ã�� ���߽��ϴ�.");
        }
    }

    /// <summary>���� �� ������Ʈ�� �����ϰ� ���ο� Ÿ���� ���� �� public �ʵ� �⺻���� �о�´�.</summary>
    public void AttachAndRead(string enemyTypeName)
    {
        // ���� �� ����
        if (currentEnemyComp != null)
        {
            Destroy(currentEnemyComp);
            currentEnemyComp = null;
        }

        // Ÿ�� ã�� (��ҹ��� ����)
        var asm = typeof(EnemyDataManager).Assembly;
        var type = asm.GetTypes()
            .FirstOrDefault(t =>
                typeof(MonoBehaviour).IsAssignableFrom(t) &&
                string.Equals(t.Name, enemyTypeName, StringComparison.OrdinalIgnoreCase));

        if (type == null)
        {
            Debug.LogError($"[EnemyDataManager] Ÿ�� '{enemyTypeName}'��(��) ã�� ���߽��ϴ�. (Enemy1 ���� Ŭ������ �ʿ�)");
            return;
        }

        // ������Ʈ ����
        var comp = enemyHost.gameObject.AddComponent(type) as MonoBehaviour;
        currentEnemyComp = comp;

        // public �ʵ� �б�
        snapshot.enemyName = type.Name;
        snapshot.maxHP = GetIntField(type, comp, "maxHP", 1);
        snapshot.currentHP = GetIntField(type, comp, "currentHP", snapshot.maxHP);
        snapshot.attack = GetIntField(type, comp, "attack", 0);
        snapshot.defense = GetIntField(type, comp, "defense", 0);
        snapshot.speed = GetIntField(type, comp, "speed", 0);

        Debug.Log($"[EnemyDataManager] Attached {type.Name} @ {enemyHost.name} | HP {snapshot.currentHP}/{snapshot.maxHP}, ATK {snapshot.attack}, DEF {snapshot.defense}, SPD {snapshot.speed}");
    }

    int GetIntField(Type t, object inst, string fieldName, int fallback)
    {
        var f = t.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        if (f == null) { Debug.LogWarning($"[EnemyDataManager] {t.Name}�� '{fieldName}' �ʵ尡 �����ϴ�."); return fallback; }
        try { return Mathf.Max(0, (int)f.GetValue(inst)); }
        catch { return fallback; }
    }
}
