using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    // ▼▼▼ 추가된 변수 ▼▼▼
    private bool isStartingDialogue = false; // 대화가 '시작되는 중'인지 확인하는 플래그

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
    }

    void Update()
    {
        // ▼▼▼ 수정된 조건문 ▼▼▼
        // 대화가 활성화 상태이고, '시작 중'이 아니며, E키가 눌렸을 때
        if (isDialogueActive && !isStartingDialogue && Input.GetKeyDown(KeyCode.E))
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;
        isStartingDialogue = true; // 대화가 '시작되는 중'이라고 표시
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

        // 첫 대사가 표시된 후에는 '시작 중' 상태를 해제
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
        typingCoroutine = StartCoroutine(TypeSentence(sentence.text));
    }

    IEnumerator TypeSentence(string text)
    {
        dialogueText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator EndDialogueRoutine()
    {
        yield return new WaitForEndOfFrame();

        isDialogueActive = false;
        dialogueCanvas.SetActive(false);
    }
}