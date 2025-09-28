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

    // ���� �߰��� ���� ����
    private bool isStartingDialogue = false; // ��ȭ�� '���۵Ǵ� ��'���� Ȯ���ϴ� �÷���

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
        // ���� ������ ���ǹ� ����
        // ��ȭ�� Ȱ��ȭ �����̰�, '���� ��'�� �ƴϸ�, EŰ�� ������ ��
        if (isDialogueActive && !isStartingDialogue && Input.GetKeyDown(KeyCode.E))
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;
        isStartingDialogue = true; // ��ȭ�� '���۵Ǵ� ��'�̶�� ǥ��
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

        // ù ��簡 ǥ�õ� �Ŀ��� '���� ��' ���¸� ����
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