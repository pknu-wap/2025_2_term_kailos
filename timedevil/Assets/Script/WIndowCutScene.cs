using UnityEngine;
using System.Collections;

// 이 스크립트는 AudioSource 컴포넌트를 필수로 요구합니다.
[RequireComponent(typeof(AudioSource))]
public class WindowCutScene: MonoBehaviour
{
    [Header("1. 창문에서 할 대화")]
    public Dialogue windowDialogue;

    [Space(10)]
    [Header("2. 문에서 일어날 이벤트")]
    [Tooltip("창문 대화가 끝나고 몇 초 후에 문을 두드릴지 결정합니다.")]
    public float delayBeforeKnock = 2.0f;
    public AudioClip knockSound;
    public Dialogue doorDialogue;

    private AudioSource audioSource;
    private bool isSequenceRunning = false; // 컷씬이 중복 실행되는 것을 방지

    void Start()
    {
        // '똑똑똑' 소리를 재생할 AudioSource를 가져옵니다.
        audioSource = GetComponent<AudioSource>();
    }

    // ▼▼▼ 이 오브젝트를 '클릭'하거나 '상호작용'했을 때 호출할 함수입니다 ▼▼▼
    // (만약 플레이어가 와서 'E'키를 누르는 방식이라면, 
    //  플레이어 스크립트에서 이 함수를 호출하게 해야 합니다.)

    // 지금은 테스트하기 쉽도록 '마우스 클릭'으로 작동하게 해두겠습니다.
    private void OnMouseDown()
    {
        // 1. 컷씬이 이미 진행 중이면 아무것도 안 함
        if (isSequenceRunning) return;

        // 2. 다른 대화가 이미 진행 중이면 아무것도 안 함
        if (DialogueManager.instance.isDialogueActive) return;

        // 3. 모든 조건이 맞으면 컷씬 코루틴 시작!
        StartCoroutine(FullSequence());
    }


    // ▼▼▼ 전체 이벤트 순서(시퀀스)를 관리하는 코루틴 ▼▼▼
    IEnumerator FullSequence()
    {
        isSequenceRunning = true; // 컷씬 시작 플래그 ON

        // --- 1. 창문 대화 시작 ---
        DialogueManager.instance.StartDialogue(windowDialogue);

        // --- 2. 창문 대화가 끝날 때까지 대기 ---
        // DialogueManager의 isDialogueActive가 false가 될 때까지 매 프레임 기다립니다.
        yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);

        // --- 3. 설정한 시간(2초)만큼 대기 ---
        yield return new WaitForSeconds(delayBeforeKnock);

        // --- 4. '똑똑똑' 소리 재생 ---
        if (knockSound != null)
        {
            // 3D 사운드가 아닌 2D 사운드로 재생하려면 audioSource.PlayOneShot(knockSound) 대신
            // BGM 매니저나 사운드 매니저의 싱글톤 인스턴스를 통해 재생하는 것이 더 좋습니다.
            // 예: SoundManager.instance.PlaySFX(knockSound);
            audioSource.PlayOneShot(knockSound);
        }

        // --- 5. 문 대화 시작 ---
        DialogueManager.instance.StartDialogue(doorDialogue);

        // --- 6. 컷씬 종료 ---
        isSequenceRunning = false;

        // (선택 사항) 이 이벤트를 딱 한 번만 실행하고 싶다면,
        // 이 스크립트(또는 오브젝트)를 비활성화합니다.
        // this.enabled = false; 
        // gameObject.SetActive(false);
    }
}