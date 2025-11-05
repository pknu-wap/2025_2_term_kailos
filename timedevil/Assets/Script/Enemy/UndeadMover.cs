using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 필요

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
    public float waitAtPoint = 0.5f; // 지점 도착 후 대기 시간

    // --- private 변수 ---
    private int currentWaypointIndex = 0; // 현재 이동 중인 목표 지점의 인덱스
    private bool isMoving = false;
    private Animator animator;

    void Awake()
    {
        // 애니메이터가 있다면 가져옵니다. (선택 사항)
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // 게임이 시작될 때 'startOnPlay'가 체크되어 있으면 순찰을 바로 시작합니다.
        if (startOnPlay)
        {
            StartPatrol();
        }
    }

    // ▼▼▼ 다른 컷씬 스크립트에서 이 함수를 호출하여 순찰을 시작시킬 수 있습니다. ▼▼▼
    public void StartPatrol()
    {
        // 포인트가 설정되지 않았거나 이미 움직이는 중이면 무시
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("MobWaypointMover: 설정된 waypoints가 없습니다.");
            return;
        }
        if (isMoving) return;

        // 순찰 코루틴 시작
        StartCoroutine(PatrolCoroutine());
    }

    // ▼▼▼ 실제 순찰 로직을 처리하는 코루틴 ▼▼▼
    IEnumerator PatrolCoroutine()
    {
        isMoving = true;

        while (true) // 이 루프는 '순찰' 자체를 반복합니다. (loopPatrol이 false면 break로 탈출)
        {
            // 1. 현재 목표 지점(Transform)을 배열에서 가져옵니다.
            Transform targetPoint = waypoints[currentWaypointIndex];
            Vector3 targetPosition = targetPoint.position;

            // (선택 사항) 걷기 애니메이션 시작
            if (animator != null) animator.SetBool("isWalking", true);

            // 2. 목표 지점에 도착할 때까지 '이동' 루프
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,     // 현재 위치
                    targetPosition,         // 목표 위치
                    moveSpeed * Time.deltaTime // 이동 속도
                );
                yield return null; // 다음 프레임까지 대기
            }

            // 3. 도착 완료 (정확한 위치로 보정)
            transform.position = targetPosition;

            // (선택 사항) 멈춤 애니메이션
            if (animator != null) animator.SetBool("isWalking", false);

            // 4. 지점에서 잠시 대기
            if (waitAtPoint > 0)
            {
                yield return new WaitForSeconds(waitAtPoint);
            }

            // 5. 다음 지점을 향하도록 인덱스 증가
            currentWaypointIndex++;

            // 6. 인덱스가 배열의 끝에 도달했는지 확인
            if (currentWaypointIndex >= waypoints.Length)
            {
                if (loopPatrol)
                {
                    // 루프가 켜져 있으면 인덱스를 0으로 리셋 (처음 지점으로)
                    currentWaypointIndex = 0;
                }
                else
                {
                    // 루프가 꺼져 있으면 순찰을 종료 (while 루프 탈출)
                    break;
                }
            }
        } // while(true) 순찰 루프의 끝

        // 루프가 중단되면 (loopPatrol == false)
        isMoving = false;
    }
}