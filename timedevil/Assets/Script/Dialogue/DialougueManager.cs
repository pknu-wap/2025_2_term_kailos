using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    public bool blockInput = false;

    private AudioSource audioSource;
    public AudioClip typingSound;   // 추가됨

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

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (blockInput) return;

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

        audioSource.Stop();

        if (sentence.voiceClip != null)
        {
            audioSource.PlayOneShot(sentence.voiceClip);
        }

        typingCoroutine = StartCoroutine(TypeSentence(sentence.text));
    }

    IEnumerator TypeSentence(string text)
    {
        dialogueText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;

            if (typingSound != null)
                audioSource.PlayOneShot(typingSound);   // 추가된 부분

            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator EndDialogueRoutine()
    {
        yield return new WaitForEndOfFrame();

        isDialogueActive = false;
        dialogueCanvas.SetActive(false);

        audioSource.Stop();
    }
}
