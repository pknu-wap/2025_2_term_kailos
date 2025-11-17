using UnityEngine;
using System.Collections;

/// 이제 이 스크립트는 순찰(Waypoint) 이동 로직만 담당
/// 씬 전환 및 충돌 감지는 'EnemyBattleTrigger.cs'가 담당
public class UndeadMover : MonoBehaviour
{
    [Header("순찰 경로 설정")]
    public Transform[] waypoints;

    [Header("이동 설정")]
    public float moveSpeed = 3f;

    [Header("동작 설정")]
    public bool startOnPlay = true;
    public bool loopPatrol = true;
    public float waitAtPoint = 0.5f;

    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private Animator animator;

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

    // (순찰 코루틴은 동일 - 애니메이션 호출 부분은 주석 처리된 상태)
    IEnumerator PatrolCoroutine()
    {
        isMoving = true;
        while (true)
        {
            Transform targetPoint = waypoints[currentWaypointIndex];
            Vector3 targetPosition = targetPoint.position;
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
}