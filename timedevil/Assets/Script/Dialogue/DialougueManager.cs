using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro�� ����ϱ� ���� �ʿ�
using UnityEngine.UI; // Image�� ����ϱ� ���� �ʿ�

public class DialogueManager : MonoBehaviour
{
    // �̱��� ������ ����Ͽ� ��𼭵� ���� ������ �� �ֵ��� �ν��Ͻ� ����
    public static DialogueManager instance;

    // --- UI ��ҵ��� ���� ������ ---
    [Header("UI Elements")]
    public GameObject dialogueCanvas;     // ��ȭâ ��ü�� ��� �ִ� ĵ���� ������Ʈ
    public TextMeshProUGUI nameText;      // ĳ���� �̸��� ǥ���� �ؽ�Ʈ
    public Image portraitImage;           // ĳ���� �ʻ�ȭ�� ǥ���� �̹���
    public TextMeshProUGUI dialogueText;  // ���� ��縦 ǥ���� �ؽ�Ʈ

    // ������ ������� ó���ϱ� ���� ť(Queue)
    private Queue<Sentence> sentenceQueue;

    // ���� ��ȭ�� ���� ������ ���¸� �����ϴ� ����
    public bool isDialogueActive = false;

    // ������ ����Ǵ� Ÿ�� ȿ�� �ڷ�ƾ�� �����ϱ� ���� ����
    private Coroutine typingCoroutine;

    // ������ ���۵� �� �� ���� ȣ��Ǵ� �Լ�
    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ���� �ٲ� �ı����� �ʵ��� ����
        }
        else
        {
            Destroy(gameObject); // �̹� �ν��Ͻ��� �ִٸ� �ߺ� ���� ������ ���� �����θ� �ı�
        }

        // ť �ʱ�ȭ
        sentenceQueue = new Queue<Sentence>();
    }

    // �� �����Ӹ��� ȣ��Ǵ� �Լ�
    void Update()
    {
        // ��ȭ�� Ȱ��ȭ�� �����̰� EŰ�� ������ ���� ���� ���� �Ѿ
        if (isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            DisplayNextSentence();
        }
    }

    // ���ο� ��ȭ�� �����ϴ� �Լ�
    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;
        dialogueCanvas.SetActive(true); // ��ȭâ UI Ȱ��ȭ
        sentenceQueue.Clear(); // ���ο� ��ȭ�� ���� ���� ������ ��� ����

        // ���޹��� Dialogue ��ü ���� ��� Sentence���� ť�� �߰�
        foreach (Sentence sentence in dialogue.sentences)
        {
            sentenceQueue.Enqueue(sentence);
        }

        // ù ��° ��� ǥ��
        DisplayNextSentence();
    }

    // ���� ��縦 ǥ���ϴ� �Լ�
    public void DisplayNextSentence()
    {
        // ���� �����ִ� ��簡 ���ٸ�, ��ȭ ���� �ڷ�ƾ�� ����
        if (sentenceQueue.Count == 0)
        {
            StartCoroutine(EndDialogueRoutine());
            return;
        }

        // ������ ���� ���̴� Ÿ�� ȿ���� �ִٸ� ����
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // ť���� ���� ��縦 ������
        Sentence sentence = sentenceQueue.Dequeue();

        // UI ��ҵ��� ���� ��翡 �°� ������Ʈ
        nameText.text = sentence.characterName;
        portraitImage.sprite = sentence.characterPortrait;

        // ���ο� Ÿ�� ȿ�� �ڷ�ƾ ����
        typingCoroutine = StartCoroutine(TypeSentence(sentence.text));
    }

    // �ؽ�Ʈ�� �� ���ھ� Ÿ�ڱ�ó�� ����ϴ� ȿ���� �ִ� �ڷ�ƾ
    IEnumerator TypeSentence(string text)
    {
        dialogueText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // ���ڰ� ��Ÿ���� �ӵ�
        }
    }

    // ��ȭ�� �����ϴ� �ڷ�ƾ
    IEnumerator EndDialogueRoutine()
    {
        // �Է� �ߺ� ó���� ���� ����, ���� �������� ��� �۾��� ���� ������ ���
        yield return new WaitForEndOfFrame();

        // ��ȭ ���¿� UI�� ��Ȱ��ȭ
        isDialogueActive = false;
        dialogueCanvas.SetActive(false);
    }
}