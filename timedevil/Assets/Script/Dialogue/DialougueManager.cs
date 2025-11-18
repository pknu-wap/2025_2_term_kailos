using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// ▼▼▼ 1. [핵심 수정] AudioSource 컴포넌트를 이 오브젝트에 필수로 요구합니다. ▼▼▼
[RequireComponent(typeof(AudioSource))]
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI Elements")]
    public GameObject dialogueCanvas;
    public TextMeshProUGUI nameText;
    public Image portraitImage;
    public TextMeshProUGUI dialogueText;

    private Queue<Sentence> sentenceQueue;
    public bool isDialogueActive = false;
    private Coroutine typingCoroutine;
    private bool isStartingDialogue = false;

    // ▼▼▼ 2. [핵심 수정] 오디오 재생을 위한 변수 선언 ▼▼▼
    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        sentenceQueue = new Queue<Sentence>();

        // ▼▼▼ 3. [핵심 수정] AudioSource 컴포넌트를 가져오고 초기화합니다. ▼▼▼
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // 게임 시작 시 자동 재생 방지
    }

    void Update()
    {
        // (기존과 동일)
        if (isDialogueActive && !isStartingDialogue && Input.GetKeyDown(KeyCode.E))
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;
        isStartingDialogue = true;
        dialogueCanvas.SetActive(true);
        sentenceQueue.Clear();

        foreach (Sentence sentence in dialogue.sentences)
        {
            sentenceQueue.Enqueue(sentence);
        }

        StartCoroutine(StartDialogueRoutine());
    }

    private IEnumerator StartDialogueRoutine()
    {
        yield return null;
        DisplayNextSentence();
        isStartingDialogue = false;
    }

    public void DisplayNextSentence()
    {
        if (sentenceQueue.Count == 0)
        {
            StartCoroutine(EndDialogueRoutine());
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        Sentence sentence = sentenceQueue.Dequeue();
        nameText.text = sentence.characterName;
        portraitImage.sprite = sentence.characterPortrait;

        // 1. 이전 대사의 목소리가 아직 나오고 있다면 강제 중지
        audioSource.Stop();

        // 2. 이번 대사에 목소리(voiceClip)가 배정되어 있다면 재생
        if (sentence.voiceClip != null)
        {
            // PlayOneShot을 사용하면 여러 소리가 겹치지 않고 한 번만 재생됩니다.
            audioSource.PlayOneShot(sentence.voiceClip);
        }
        // ▲▲▲

        typingCoroutine = StartCoroutine(TypeSentence(sentence.text));
    }

    IEnumerator TypeSentence(string text)
    {
        // (기존과 동일)
        dialogueText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // (타이핑 속도)
        }
    }

    IEnumerator EndDialogueRoutine()
    {
        // (기존과 동일)
        yield return new WaitForEndOfFrame();

        isDialogueActive = false;
        dialogueCanvas.SetActive(false);

        // (선택 사항) 대화가 끝났을 때도 오디오를 강제 중지
        audioSource.Stop();
    }
}