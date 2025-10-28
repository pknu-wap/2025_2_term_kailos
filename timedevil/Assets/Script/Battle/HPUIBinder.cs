using System;
using System.Reflection;
using TMPro;
using UnityEngine;

/// <summary>
/// 전투 UI에 HP를 "현재/최대" 형태로 표시해 주는 바인더.
/// - PlayerData는 직접 참조(SerializeField 또는 런타임 주입)
/// - Enemy는 컴포넌트(예: Enemy1)에서 public int currentHP, maxHP를 반사로 읽는다
/// </summary>
public class HPUIBinder : MonoBehaviour
{
    [Header("Player UI")]
    [SerializeField] private TMP_Text playerHpText;   // 예: "HP : 32 / 50"

    [Header("Enemy UI")]
    [SerializeField] private TMP_Text enemyHpText;    // 예: "HP : 12 / 60"

    [Header("Sources")]
    [SerializeField] private PlayerData playerData;   // 없으면 Start에서 시도해서 주입

    // Enemy 컴포넌트(Enemy1, Enemy2 등). 필드명을 반사로 읽는다.
    private MonoBehaviour enemyComp;
    private FieldInfo enemyCurHpField;
    private FieldInfo enemyMaxHpField;

    // ---------------- Public API ----------------

    /// <summary>플레이어 데이터(런타임) 바인딩. 없으면 SerializeField 값 사용.</summary>
    public void BindPlayer(PlayerData data)
    {
        playerData = data;
    }

    /// <summary>적 컴포넌트 바인딩(Enemy1 등). public int currentHP, maxHP 필드를 찾는다.</summary>
    public void BindEnemy(MonoBehaviour enemy)
    {
        enemyComp = enemy;
        CacheEnemyHPFields(enemyComp);
    }

    /// <summary>즉시 UI 새로고침.</summary>
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
        // 플레이어 데이터가 비어 있으면 런타임 싱글톤/컨텍스트에서 시도
        if (playerData == null)
        {
            // 이미 가지고 있는 런타임 관리자가 있으면 거기서 꺼내 쓰세요.
            // 예: PlayerDataRuntime.Instance?.Data
            var runtime = FindObjectOfType<PlayerDataRuntime>();
            if (runtime != null) playerData = runtime.Data;
        }

        // 씬 들어오자마자 한 번 갱신
        Refresh();
    }

    private void Update()
    {
        // 값이 자주 바뀔 수 있으니 간단히 매 프레임 갱신 (원하면 제거하고 이벤트 기반으로 호출)
        Refresh();
    }

    // ---------------- Helpers ----------------

    private void CacheEnemyHPFields(MonoBehaviour comp)
    {
        enemyCurHpField = null;
        enemyMaxHpField = null;

        if (comp == null) return;

        var t = comp.GetType();
        // Enemy1.cs에서 public int currentHP, maxHP 라고 했으므로 이름 그대로 찾는다.
        enemyCurHpField = t.GetField("currentHP", BindingFlags.Public | BindingFlags.Instance);
        enemyMaxHpField = t.GetField("maxHP", BindingFlags.Public | BindingFlags.Instance);

        if (enemyCurHpField == null || enemyMaxHpField == null)
        {
            Debug.LogWarning($"[HPUIBinder] Enemy '{t.Name}'에서 currentHP / maxHP 필드를 찾지 못했습니다.");
        }
    }
}
