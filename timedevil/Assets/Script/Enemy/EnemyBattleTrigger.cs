using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyBattleTrigger : MonoBehaviour
{
    [Header("몬스터 ID (EnemySO.enemyId)")]
    public string enemyID_ToLoad = "Enemy1_Undead";

    [Header("배틀 씬 이름")]
    public string battleSceneName = "BattleScene";

    private bool isTransitioning = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning || PlayerReturnContext.IsInGracePeriod) return;

        var player = other.GetComponent<PlayerAction>();
        if (!player) return;

        isTransitioning = true;

        // 플레이어 조작 잠금(선택)
        if (GameManager.Instance != null) GameManager.Instance.isAction = true;

        // NPC 이동 중지(선택)
        var mover = GetComponent<UndeadMover>();
        if (mover) mover.StopAllCoroutines();

        // 모든 씬 이동/저장 처리는 서비스에서
        SceneTravelService.GoToBattle(battleSceneName, enemyID_ToLoad, player.transform, transform);
    }
}
