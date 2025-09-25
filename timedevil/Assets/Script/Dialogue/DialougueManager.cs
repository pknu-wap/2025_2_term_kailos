using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필요
using UnityEngine.UI; // Image를 사용하기 위해 필요

public class DialogueManager : MonoBehaviour
{
    // 싱글톤 패턴을 사용하여 어디서든 쉽게 접근할 수 있도록 인스턴스 생성
    public static DialogueManager instance;

    // --- UI 요소들을 담을 변수들 ---
    [Header("UI Elements")]
    public GameObject dialogueCanvas;     // 대화창 전체를 담고 있는 캔버스 오브젝트
    public TextMeshProUGUI nameText;      // 캐릭터 이름을 표시할 텍스트
    public Image portraitImage;           // 캐릭터 초상화를 표시할 이미지
    public TextMeshProUGUI dialogueText;  // 실제 대사를 표시할 텍스트

    // 대사들을 순서대로 처리하기 위한 큐(Queue)
    private Queue<Sentence> sentenceQueue;

    // 현재 대화가 진행 중인지 상태를 저장하는 변수
    public bool isDialogueActive = false;

    // 이전에 실행되던 타자 효과 코루틴을 제어하기 위한 변수
    private Coroutine typingCoroutine;

    // 게임이 시작될 때 한 번만 호출되는 함수
    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 있다면 중복 생성 방지를 위해 스스로를 파괴
        }

        // 큐 초기화
        sentenceQueue = new Queue<Sentence>();
    }

    // 매 프레임마다 호출되는 함수
    void Update()
    {
        // 대화가 활성화된 상태이고 E키가 눌렸을 때만 다음 대사로 넘어감
        if (isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            DisplayNextSentence();
        }
    }

    // 새로운 대화를 시작하는 함수
    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;
        dialogueCanvas.SetActive(true); // 대화창 UI 활성화
        sentenceQueue.Clear(); // 새로운 대화를 위해 이전 대사들을 모두 지움

        // 전달받은 Dialogue 객체 안의 모든 Sentence들을 큐에 추가
        foreach (Sentence sentence in dialogue.sentences)
        {
            sentenceQueue.Enqueue(sentence);
        }

        // 첫 번째 대사 표시
        DisplayNextSentence();
    }

    // 다음 대사를 표시하는 함수
    public void DisplayNextSentence()
    {
        // 만약 남아있는 대사가 없다면, 대화 종료 코루틴을 시작
        if (sentenceQueue.Count == 0)
        {
            StartCoroutine(EndDialogueRoutine());
            return;
        }

        // 이전에 실행 중이던 타자 효과가 있다면 멈춤
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 큐에서 다음 대사를 꺼내옴
        Sentence sentence = sentenceQueue.Dequeue();

        // UI 요소들을 현재 대사에 맞게 업데이트
        nameText.text = sentence.characterName;
        portraitImage.sprite = sentence.characterPortrait;

        // 새로운 타자 효과 코루틴 시작
        typingCoroutine = StartCoroutine(TypeSentence(sentence.text));
    }

    // 텍스트를 한 글자씩 타자기처럼 출력하는 효과를 주는 코루틴
    IEnumerator TypeSentence(string text)
    {
        dialogueText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // 글자가 나타나는 속도
        }
    }

    // 대화를 종료하는 코루틴
    IEnumerator EndDialogueRoutine()
    {
        // 입력 중복 처리를 막기 위해, 현재 프레임의 모든 작업이 끝날 때까지 대기
        yield return new WaitForEndOfFrame();

        // 대화 상태와 UI를 비활성화
        isDialogueActive = false;
        dialogueCanvas.SetActive(false);
    }
}