using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// 몬스터에 붙어서 플레이어와의 충돌을 감지하고,
/// 'ObjectNameRuntime' 및 'PlayerReturnContext'에 데이터를 저장한 뒤,
/// 배틀 씬을 로드하는 '씬 로더' 역할
[RequireComponent(typeof(Collider2D))] // 충돌 감지를 위해 콜라이더 필수
public class EnemyBattleTrigger : MonoBehaviour
{
    [Header("1. 몬스터 데이터 (ID)")]
    [Tooltip("피드백: 'Undead -> Enemy1'이 되도록 설정.\n배틀 씬에서 불러올 몬스터의 ID (SO의 이름 등)")]
    public string enemyID_ToLoad = "Enemy1_Undead"; // 예시 ID

    [Header("2. 씬 로더 설정")]
    [Tooltip("로드할 배틀씬의 이름")]
    public string battleSceneName = "BattleScene";

    private bool isTransitioning = false; // 중복 씬 전환 방지

    /// 플레이어와 충돌했는지 감지 (UndeadMover에서 이 로직을 가져옴)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 씬 전환 중이거나, '무적 시간' 중이면 무시
        if (isTransitioning || PlayerReturnContext.IsInGracePeriod)
        {
            return;
        }

        // 2. 부딪힌 것이 플레이어인지 확인
        PlayerAction player = other.GetComponent<PlayerAction>();
        if (player != null)
        {
            Debug.Log($"[EnemyBattleTrigger] 플레이어와 충돌! '{enemyID_ToLoad}' 배틀 씬을 로드합니다.");
            StartCoroutine(StartBattleSequence(player.transform));
        }
    }

    /// 모든 데이터를 저장하고 씬을 전환하는 코루틴
    IEnumerator StartBattleSequence(Transform playerTransform)
    {
        isTransitioning = true;

        // 1. 플레이어 조작 비활성화
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isAction = true;
        }

        // 2. (선택 사항) 몬스터의 'UndeadMover'를 멈춤
        UndeadMover mover = GetComponent<UndeadMover>();
        if (mover != null)
        {
            mover.StopAllCoroutines(); // 순찰 코루틴 중지
        }

        // --- 3. (피드백) 데이터 저장 ---

        // 3a. (신규) 'ObjectNameRuntime'에 몬스터 ID 저장
        if (ObjectNameRuntime.Instance != null)
        {
            ObjectNameRuntime.Instance.SetEnemyToLoad(enemyID_ToLoad);
        }
        else
        {
            Debug.LogError("[EnemyBattleTrigger] ObjectNameRuntime.Instance가 null입니다! 몬스터 ID를 저장할 수 없습니다.");
        }

        // 3b. (기존) 'PlayerReturnContext'에 복귀 위치 저장
        PlayerReturnContext.ReturnSceneName = SceneManager.GetActiveScene().name;
        PlayerReturnContext.HasReturnPosition = true;
        PlayerReturnContext.ReturnPosition = playerTransform.position;
        PlayerReturnContext.MonsterReturnPosition = this.transform.position;
        PlayerReturnContext.MonsterNameInScene = this.gameObject.name;

        // --- 4. 씬 전환 ---
        // (피드백: "SceneFader.으로 자연스럽게 넘어가게")
        SceneFader.instance.LoadSceneWithFade(battleSceneName);

        yield return null;
    }
}