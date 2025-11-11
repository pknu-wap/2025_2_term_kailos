using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class UndeadMover : MonoBehaviour
{
    // (순찰, 이동, 동작 설정 변수들은 그대로)
    [Header("순찰 경로 설정")]
    public Transform[] waypoints;
    [Header("이동 설정")]
    public float moveSpeed = 3f;
    [Header("동작 설정")]
    public bool startOnPlay = true;
    public bool loopPatrol = true;
    public float waitAtPoint = 0.5f;
    [Header("배틀 설정")]
    public string battleSceneName = "BattleScene";

    // (private 변수들도 그대로)
    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private Animator animator;
    private bool isTransitioning = false;

    // (Awake, Start, StartPatrol, PatrolCoroutine 함수는 그대로)
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


    // (OnTriggerEnter2D는 그대로)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning) return;
        if (other.GetComponent<PlayerAction>() != null)
        {
            Debug.Log("플레이어와 충돌! 페이드와 함께 배틀 씬을 로드합니다.");
            StartCoroutine(StartBattleSequence(other.transform));
        }
    }

    // ▼▼▼ (핵심 수정) SceneFader 호출 방식 변경 ▼▼▼
    IEnumerator StartBattleSequence(Transform playerTransform)
    {
        isTransitioning = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.isAction = true;
        }
        StopAllCoroutines();
        isMoving = false;

        // --- 3. '돌아올 정보'를 정적 클래스에 저장 ---
        PlayerReturnContext.ReturnSceneName = SceneManager.GetActiveScene().name;
        PlayerReturnContext.ReturnPosition = playerTransform.position;
        PlayerReturnContext.HasReturnPosition = true;

        // --- 4. (수정) SceneFader에게 "알아서 페이드아웃하고 씬 로드해"라고 맡김 ---
        // (UndeadMover가 직접 Fade(1f)를 호출하는 코드를 삭제합니다)
        SceneFader.instance.LoadSceneWithFade(battleSceneName);

        // (코루틴이므로 yield return은 필요 없습니다)
        yield return null;
    }
}