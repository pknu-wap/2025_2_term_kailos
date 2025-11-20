using UnityEngine;
using System.Collections;
using Cinemachine; // Cinemachine을 사용하기 위해 필요

public class BedCutscene : MonoBehaviour, IInteractable
{
    [Header("1. 이벤트 순서별 대화")]
    public Dialogue[] dialogues; // 순서대로 4개의 대화를 넣을 배열

    [Header("2. 카메라 및 효과")]
    public CinemachineVirtualCamera virtualCamera; // 제어할 '눈' (Virtual Camera)
    public AudioClip thumpSound; // '털썩' 사운드
    public string nextSceneName; // 마지막에 넘어갈 씬 이름

    private int interactionCount = 0; // 몇 번째 상호작용인지 기억하는 변수
    private AudioSource audioSource;

    private void Start()
    {
        // 침대 오브젝트에 있는 AudioSource를 가져옴
        audioSource = GetComponent<AudioSource>();
    }

    // PlayerAction 스크립트가 이 함수를 호출합니다.
    public void Interact()
    {
        // 상호작용 횟수에 따라 다른 행동을 실행
        switch (interactionCount)
        {
            case 0: // 첫 번째 상호작용
                DialogueManager.instance.StartDialogue(dialogues[0]);
                interactionCount++; // 다음 상호작용을 위해 카운트 1 증가
                break;
            case 1: // 두 번째 상호작용
                StartCoroutine(FadeZoomDialogue(dialogues[1], 4.0f)); // 카메라 크기 4.5로 줌인
                interactionCount++;
                break;
            case 2: // 세 번째 상호작용
                StartCoroutine(FadeZoomDialogue(dialogues[2], 3.0f)); // 더 줌인
                interactionCount++;
                break;
            case 3: // 네 번째 상호작용
                StartCoroutine(FadeZoomDialogue(dialogues[3], 2.0f)); // 더 줌인
                interactionCount++;
                break;
            case 4: // 마지막 상호작용
                StartCoroutine(FinalSequence());
                break;
        }
    }

    // 페이드, 줌, 대화를 순서대로 처리하는 기능
    IEnumerator FadeZoomDialogue(Dialogue dialogue, float targetZoomSize)
    {
        // 1. 화면을 검게 (페이드 아웃)
        yield return StartCoroutine(SceneFader.instance.Fade(1f));

        // 2. 화면이 검은 동안 '눈'의 렌즈 크기를 조절해 줌인
        if (virtualCamera != null)
        {
            virtualCamera.m_Lens.OrthographicSize = targetZoomSize;
        }

        // 3. 대화 시작
        DialogueManager.instance.StartDialogue(dialogue);

        // 4. 다시 화면을 밝게 (페이드 인)
        yield return StartCoroutine(SceneFader.instance.Fade(0f));
    }

    // 마지막 시퀀스(암전 -> 소리 -> 씬 전환)를 처리하는 기능
    IEnumerator FinalSequence()
    {
        // 1. 완전히 어둡게
        yield return StartCoroutine(SceneFader.instance.Fade(1f));

        // 4. 다음 씬으로 전환
        SceneFader.instance.LoadSceneWithFade(nextSceneName);
    }
}