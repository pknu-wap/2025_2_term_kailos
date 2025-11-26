using UnityEngine;
using System.Collections;
using Cinemachine;

public class DoorTransition : MonoBehaviour, IInteractable
{
    // ... (기존 변수들 동일) ...
    [Header("이동할 목표 지점")]
    public Transform targetPoint;

    [Header("카메라 설정")]
    public CinemachineVirtualCamera virtualCamera;
    public float newCameraSize = 8f;

    [Header("BGM 설정")]
    public AudioClip newBGM;

    // ▼▼▼ [추가됨] 음악 중복 재생 방지용 키 ▼▼▼
    [Header("음악 반복 방지")]
    [Tooltip("SceneMusicStarter에 적은 'Unique Key'와 똑같이 적으세요.")]
    public string stopMusicKey = "Chapter1_Intro_BGM";
    // ▲▲▲▲▲▲

    private PlayerAction player;
    private bool isTransitioning = false;

    // ... (Start, Interact 함수는 기존과 동일) ...
    void Start() { player = FindObjectOfType<PlayerAction>(); }
    public void Interact() { if (!isTransitioning) StartCoroutine(TransitionCoroutine()); }

    IEnumerator TransitionCoroutine()
    {
        isTransitioning = true;
        if (GameManager.Instance != null) GameManager.Instance.isAction = true;

        if (SceneFader.instance != null) yield return StartCoroutine(SceneFader.instance.Fade(1f));

        // --- 화면이 검은 상태 ---

        // 1. BGM 변경 (기존 로직)
        if (newBGM != null && BGMManager.instance != null)
        {
            BGMManager.instance.PlayBGM(newBGM);
        }

        // ▼▼▼ [핵심 추가] "이제 처음 브금은 틀지 마"라고 도장 쾅! ▼▼▼
        if (!string.IsNullOrEmpty(stopMusicKey))
        {
            // stopMusicKey 이름으로 '1'을 저장합니다.
            PlayerPrefs.SetInt(stopMusicKey, 1);
            PlayerPrefs.Save(); // 저장 확정
        }
        // ▲▲▲▲▲▲

        // 2. 플레이어 이동
        player.transform.position = targetPoint.position;

        // 3. 카메라 설정
        if (virtualCamera != null)
        {
            virtualCamera.m_Lens.OrthographicSize = newCameraSize;
            virtualCamera.Follow = player.transform;
            CinemachineConfiner2D confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
            if (confiner != null) confiner.enabled = true;
        }

        yield return null;

        if (SceneFader.instance != null) yield return StartCoroutine(SceneFader.instance.Fade(0f));

        if (GameManager.Instance != null) GameManager.Instance.isAction = false;
        isTransitioning = false;
    }
}