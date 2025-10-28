using System;
using System.Reflection;
using TMPro;
using UnityEngine;

/// <summary>
/// ���� UI�� HP�� "����/�ִ�" ���·� ǥ���� �ִ� ���δ�.
/// - PlayerData�� ���� ����(SerializeField �Ǵ� ��Ÿ�� ����)
/// - Enemy�� ������Ʈ(��: Enemy1)���� public int currentHP, maxHP�� �ݻ�� �д´�
/// </summary>
public class HPUIBinder : MonoBehaviour
{
    [Header("Player UI")]
    [SerializeField] private TMP_Text playerHpText;   // ��: "HP : 32 / 50"

    [Header("Enemy UI")]
    [SerializeField] private TMP_Text enemyHpText;    // ��: "HP : 12 / 60"

    [Header("Sources")]
    [SerializeField] private PlayerData playerData;   // ������ Start���� �õ��ؼ� ����

    // Enemy ������Ʈ(Enemy1, Enemy2 ��). �ʵ���� �ݻ�� �д´�.
    private MonoBehaviour enemyComp;
    private FieldInfo enemyCurHpField;
    private FieldInfo enemyMaxHpField;

    // ---------------- Public API ----------------

    /// <summary>�÷��̾� ������(��Ÿ��) ���ε�. ������ SerializeField �� ���.</summary>
    public void BindPlayer(PlayerData data)
    {
        playerData = data;
    }

    /// <summary>�� ������Ʈ ���ε�(Enemy1 ��). public int currentHP, maxHP �ʵ带 ã�´�.</summary>
    public void BindEnemy(MonoBehaviour enemy)
    {
        enemyComp = enemy;
        CacheEnemyHPFields(enemyComp);
    }

    /// <summary>��� UI ���ΰ�ħ.</summary>
    public void Refresh()
    {
        // Player
        if (playerHpText != null && playerData != null)
        {
            playerHpText.text = $"HP : {playerData.currentHP} / {playerData.maxHP}";
        }

        // Enemy
        if (enemyHpText != null && enemyComp != null && enemyCurHpField != null && enemyMaxHpField != null)
        {
            int cur = Mathf.Max(0, (int)enemyCurHpField.GetValue(enemyComp));
            int max = Mathf.Max(1, (int)enemyMaxHpField.GetValue(enemyComp));
            enemyHpText.text = $"HP : {cur} / {max}";
        }
    }

    // ---------------- Unity lifecycle ----------------

    private void Start()
    {
        // �÷��̾� �����Ͱ� ��� ������ ��Ÿ�� �̱���/���ؽ�Ʈ���� �õ�
        if (playerData == null)
        {
            // �̹� ������ �ִ� ��Ÿ�� �����ڰ� ������ �ű⼭ ���� ������.
            // ��: PlayerDataRuntime.Instance?.Data
            var runtime = FindObjectOfType<PlayerDataRuntime>();
            if (runtime != null) playerData = runtime.Data;
        }

        // �� �����ڸ��� �� �� ����
        Refresh();
    }

    private void Update()
    {
        // ���� ���� �ٲ� �� ������ ������ �� ������ ���� (���ϸ� �����ϰ� �̺�Ʈ ������� ȣ��)
        Refresh();
    }

    // ---------------- Helpers ----------------

    private void CacheEnemyHPFields(MonoBehaviour comp)
    {
        enemyCurHpField = null;
        enemyMaxHpField = null;

        if (comp == null) return;

        var t = comp.GetType();
        // Enemy1.cs���� public int currentHP, maxHP ��� �����Ƿ� �̸� �״�� ã�´�.
        enemyCurHpField = t.GetField("currentHP", BindingFlags.Public | BindingFlags.Instance);
        enemyMaxHpField = t.GetField("maxHP", BindingFlags.Public | BindingFlags.Instance);

        if (enemyCurHpField == null || enemyMaxHpField == null)
        {
            Debug.LogWarning($"[HPUIBinder] Enemy '{t.Name}'���� currentHP / maxHP �ʵ带 ã�� ���߽��ϴ�.");
        }
    }
}
