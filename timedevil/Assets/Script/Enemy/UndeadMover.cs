using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요

// 이 스크립트는 2D 콜라이더를 필수로 요구합니다.
[RequireComponent(typeof(Collider2D))]
public class UndeadMover : MonoBehaviour
{
    [Header("순찰 경로 설정")]
    [Tooltip("몹이 순서대로 방문할 지점(빈 오브젝트)들의 배열입니다.")]
    public Transform[] waypoints; // 여러 개의 포인트를 담을 배열

    [Header("이동 설정")]
    public float moveSpeed = 3f;

    [Header("동작 설정")]
    [Tooltip("체크하면 게임 시작 시 바로 순찰을 시작합니다.")]
    public bool startOnPlay = true;
    [Tooltip("체크하면 마지막 지점 도착 후 첫 지점으로 돌아가 순찰을 반복합니다.")]
    public bool loopPatrol = true;
    [Tooltip("각 지점에 도착한 후 다음 지점으로 출발하기 전 대기하는 시간(초)입니다.")]
    public float waitAtPoint = 0.5f;

    [Header("배틀 설정")]
    [Tooltip("충돌 시 진입할 배틀씬의 이름")]
    public string battleSceneName = "BattleScene";

    // --- private 변수 ---
    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private Animator animator;

    // ▼▼▼ [핵심 추가] 이 한 줄이 없어서 오류가 발생했습니다 ▼▼▼
    private bool isTransitioning = false; // 씬 전환 중복 방지 플래그
    // ▲▲▲

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (startOnPlay)
        {
            StartPatrol();
        }
    }

    public void StartPatrol()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("MobWaypointMover: 설정된 waypoints가 없습니다.");
            return;
        }
        if (isMoving) return;

        StartCoroutine(PatrolCoroutine());
    }

    // (순찰 코루틴 - 애니메이션 호출 부분은 주석 처리된 상태)
    IEnumerator PatrolCoroutine()
    {
        isMoving = true;
        while (true)
        {
            Transform targetPoint = waypoints[currentWaypointIndex];
            Vector3 targetPosition = targetPoint.position;
            // if (animator != null) animator.SetBool("isWalking", true); 
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );
                if (GetComponent<SpriteRenderer>() != null)
                {
                    if (targetPosition.x > transform.position.x)
                        GetComponent<SpriteRenderer>().flipX = false;
                    else if (targetPosition.x < transform.position.x)
                        GetComponent<SpriteRenderer>().flipX = true;
                }
                yield return null;
            }
            transform.position = targetPosition;
            // if (animator != null) animator.SetBool("isWalking", false); 
            if (waitAtPoint > 0)
            {
                yield return new WaitForSeconds(waitAtPoint);
            }
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                if (loopPatrol)
                {
                    currentWaypointIndex = 0;
                }
                else
                {
                    break;
                }
            }
        }
        isMoving = false;
    }

    // ▼▼▼ (수정) 무적 시간(GracePeriod) 체크 로직 포함 ▼▼▼

    /// <summary>
    /// 이 오브젝트의 'IsTrigger' 콜라이더에 무언가 들어왔을 때
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. (수정) 씬 전환 중이거나, '무적 시간' 중이면 무시
        if (isTransitioning || PlayerReturnContext.IsInGracePeriod)
        {
            return;
        }

        // 2. 부딪힌 것이 플레이어인지 확인
        if (other.GetComponent<PlayerAction>() != null)
        {
            Debug.Log("플레이어와 충돌! 페이드와 함께 배틀 씬을 로드합니다.");
            // 3. 배틀 시작 코루틴 실행
            StartCoroutine(StartBattleSequence(other.transform));
        }
    }

    /// <summary>
    /// '돌아올 정보'를 저장하고 배틀씬을 로드하는 코루틴
    /// (SceneFader를 호출하는 최종본)
    /// </summary>
    IEnumerator StartBattleSequence(Transform playerTransform)
    {
        isTransitioning = true; // 씬 전환 시작 (플래그 ON)

        // 1. 플레이어 조작 비활성화
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isAction = true;
        }
        StopAllCoroutines();
        isMoving = false;

        // 2. '돌아올 정보'를 정적 클래스에 저장
        PlayerReturnContext.ReturnSceneName = SceneManager.GetActiveScene().name;
        PlayerReturnContext.ReturnPosition = playerTransform.position;
        PlayerReturnContext.HasReturnPosition = true;

        // 3. SceneFader에게 "알아서 페이드아웃하고 씬 로드해"라고 맡김
        SceneFader.instance.LoadSceneWithFade(battleSceneName);

        yield return null;
    }
}