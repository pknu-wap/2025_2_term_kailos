using UnityEngine;
using System.Collections;
using Cinemachine;

public class BedCutscene : MonoBehaviour, IInteractable
{
    [Header("1. 이벤트 순서별 대화")]
    public Dialogue[] dialogues;

    [Header("2. 카메라 및 효과")]
    public CinemachineVirtualCamera virtualCamera;
    public AudioClip thumpSound;
    public string nextSceneName;

    [Header("3. 자동대사 넘김 설정")]
    public float autoAdvanceDelay = 1.5f;     // 모든 단계 사이 딜레이

    private AudioSource audioSource;
    private bool isRunning = false; // 이미 실행 중인지 체크

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        if (isRunning) return; // 중복 실행 방지
        isRunning = true;

        StartCoroutine(RunAllSequences());
    }

    private IEnumerator RunAllSequences()
    {
        for (int i = 0; i < dialogues.Length; i++)
        {
            float targetZoom = 5f; // 기본 줌
            if (i == 1) targetZoom = 4f;
            else if (i == 2) targetZoom = 3f;
            else if (i == 3) targetZoom = 2f;

            yield return StartCoroutine(FadeZoomDialogue(dialogues[i], targetZoom));
        }

        // 마지막 씬 전환
        yield return StartCoroutine(FinalSequence());
    }

    IEnumerator FadeZoomDialogue(Dialogue dialogue, float targetZoomSize)
    {
        // 화면 암전
        yield return StartCoroutine(SceneFader.instance.Fade(1f));

        // 카메라 줌
        if (virtualCamera != null)
            virtualCamera.m_Lens.OrthographicSize = targetZoomSize;

        // 대사 시작
        DialogueManager.instance.StartDialogue(dialogue);

        // 화면 밝히기
        yield return StartCoroutine(SceneFader.instance.Fade(0f));

        // 자동으로 한 줄씩 진행
        while (DialogueManager.instance.isDialogueActive)
        {
            yield return new WaitForSeconds(autoAdvanceDelay);
            DialogueManager.instance.DisplayNextSentence();
        }
    }

    IEnumerator FinalSequence()
    {
        // 화면 완전히 암전 후 씬 전환
        yield return StartCoroutine(SceneFader.instance.Fade(1f));
        SceneFader.instance.LoadSceneWithFade(nextSceneName);
    }
}
