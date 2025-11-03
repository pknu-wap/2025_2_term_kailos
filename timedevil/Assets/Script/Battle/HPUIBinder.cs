// Assets/Script/Battle/HPUIBinder.cs
using TMPro;
using UnityEngine;

/// <summary>
/// 전투 UI에 HP를 "현재/최대" 형태로 표시.
/// - 플레이어: PlayerDataRuntime.Data
/// - 적: EnemyRuntime (SO 기반)
/// </summary>
public class HPUIBinder : MonoBehaviour
{
    [Header("Player UI")]
    [SerializeField] private TMP_Text playerHpText;   // "HP : 100 / 100"

    [Header("Enemy UI")]
    [SerializeField] private TMP_Text enemyHpText;    // "HP : 60 / 60"

    [Header("Sources")]
    [SerializeField] private PlayerData playerData;   // PlayerDataRuntime.Data 주입
    [SerializeField] private EnemyRuntime enemyRuntime;

    // --------- Public API ---------
    public void BindPlayer(PlayerData data) => playerData = data;
    public void BindEnemyRuntime(EnemyRuntime rt) => enemyRuntime = rt;

    public void Refresh()
    {
        // Player
        if (playerHpText != null && playerData != null)
            playerHpText.text = $"HP : {playerData.currentHP} / {playerData.maxHP}";

        // Enemy
        if (enemyHpText != null && enemyRuntime != null)
            enemyHpText.text = $"HP : {enemyRuntime.CurrentHP} / {enemyRuntime.MaxHP}";
    }

    private void Start()
    {
        // 없으면 자동 주입 시도
        if (playerData == null)
        {
            var pdr = PlayerDataRuntime.Instance ?? FindObjectOfType<PlayerDataRuntime>();
            if (pdr != null) playerData = pdr.Data;
        }
        if (enemyRuntime == null)
            enemyRuntime = EnemyRuntime.Instance ?? FindObjectOfType<EnemyRuntime>();

        Refresh();
    }

    private void Update()
    {
        // 간단하게 매 프레임 갱신 (원하면 이벤트 기반으로 바꿔도 됨)
        Refresh();
    }
}
