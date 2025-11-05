using UnityEngine;
using System.Collections;
using Cinemachine; // 시네머신 카메라를 제어하기 위해 필수!

public class DoorTransition : MonoBehaviour, IInteractable
{
    [Header("이동할 목표 지점")]
    [Tooltip("플레이어가 페이드 아웃 후 이동될 위치 (씬에 있는 빈 오브젝트)")]
    public Transform targetPoint;

    [Header("카메라 설정")]
    [Tooltip("이동 후 제어할 시네머신 가상 카메라")]
    public CinemachineVirtualCamera virtualCamera;

    [Tooltip("이동 후 설정할 카메라의 새 Orthographic Size")]
    public float newCameraSize = 8f; // 원하는 줌 레벨

    // --- private 변수 ---
    private PlayerAction player; // 플레이어 참조
    private bool isTransitioning = false; // 컷씬 중복 실행 방지

    void Start()
    {
        // 씬에서 플레이어 오브젝트를 찾아 참조를 저장해 둡니다.
        player = FindObjectOfType<PlayerAction>();
        if (player == null)
        {
            Debug.LogError("[DoorTransition] 씬에서 'PlayerAction' 스크립트를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 플레이어가 E키 등으로 호출하는 진입점
    /// </summary>
    public void Interact()
    {
        // (null 체크 및 중복 실행 방지 코드는 이전과 동일)
        if (targetPoint == null)
        {
            Debug.LogWarning("[DoorTransition] 'Target Point'가 설정되지 않았습니다.");
            return;
        }
        if (player == null) return;
        if (isTransitioning || (DialogueManager.instance != null && DialogueManager.instance.isDialogueActive))
        {
            return;
        }
        StartCoroutine(TransitionCoroutine());
    }

    /// <summary>
    /// 페이드 -> 이동/카메라설정 -> 페이드 순서를 관리하는 코루틴
    /// </summary>
    IEnumerator TransitionCoroutine()
    {
        isTransitioning = true;

        // 1. 플레이어 조작 비활성화
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isAction = true;
        }

        // 2. 페이드 아웃
        yield return StartCoroutine(SceneFader.instance.Fade(1f));

        // --- 화면이 검은 상태 (모든 변경은 여기서) ---

        // 3. 플레이어 위치를 'targetPoint'의 위치로 강제 이동
        player.transform.position = targetPoint.position;

        // 4. 카메라 설정 변경
        if (virtualCamera != null)
        {
            // 4a. 카메라 사이즈 변경
            virtualCamera.m_Lens.OrthographicSize = newCameraSize;

            // 4b. 카메라 Follow 타겟을 플레이어로 지정
            virtualCamera.Follow = player.transform;

            // ▼▼▼ 4c. (추가된 부분) CinemachineConfiner2D 활성화 ▼▼▼
            CinemachineConfiner2D confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
            if (confiner != null)
            {
                confiner.enabled = true; // 컨파이너(경계)를 켭니다.
            }
            else
            {
                Debug.LogWarning("[DoorTransition] 'Virtual Camera' 오브젝트에 'CinemachineConfiner2D' 컴포넌트가 없습니다.");
            }
            // ▲▲▲
        }
        else
        {
            Debug.LogWarning("[DoorTransition] 'Virtual Camera'가 연결되지 않았습니다! (인스펙터 창 확인 필요)");
        }

        // 5. 카메라가 새 Follow 타겟을 따라잡도록 1프레임 대기
        yield return null;

        // 6. 다시 화면 밝게 (페이드 인)
        yield return StartCoroutine(SceneFader.instance.Fade(0f));

        // --- 이동 완료 ---

        // 7. 플레이어 조작 다시 활성화
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isAction = false;
        }

        isTransitioning = false;
    }
}