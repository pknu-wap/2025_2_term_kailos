using UnityEngine;
using System.Collections;
using Cinemachine;

public class Chapter1_OpeningSequence : MonoBehaviour
{
    [Header("오프닝 독백")]
    public Dialogue openingMonologue;

    [Header("침대 오브젝트 연결")]
    public GameObject normalBedObject;
    public GameObject lyingDownBedObject;

    [Header("카메라 설정")]
    public CinemachineVirtualCamera playerFollowCamera;

    // ▼▼▼ 여기에 '일어나는 지점' 변수 추가 ▼▼▼
    [Header("플레이어 설정")]
    [Tooltip("컷신이 끝난 후 플레이어가 나타날 위치")]
    public Transform wakeUpPoint;
    // ▲▲▲

    [Header("연출 시간 설정")]
    public float delayBeforeSleep = 2f;
    public float delayBeforeWakeUp = 2f;

    private SpriteRenderer playerSprite;
    private PlayerAction playerAction;

    void Start()
    {
        playerAction = FindObjectOfType<PlayerAction>();
        if (playerAction != null)
        {
            playerSprite = playerAction.GetComponent<SpriteRenderer>();
            playerSprite.enabled = false;
        }

        if (playerFollowCamera != null)
        {
            playerFollowCamera.Follow = null;
        }

        normalBedObject.SetActive(false);
        lyingDownBedObject.SetActive(true);

        StartCoroutine(OpeningSequence());
    }

    IEnumerator OpeningSequence()
    {
        // ... (대화, 페이드인/아웃 등 기존 연출은 그대로) ...
        DialogueManager.instance.StartDialogue(openingMonologue);
        yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);
        yield return new WaitForSeconds(delayBeforeSleep);
        yield return StartCoroutine(SceneFader.instance.Fade(1f));
        yield return new WaitForSeconds(delayBeforeWakeUp);
        yield return StartCoroutine(SceneFader.instance.Fade(0f));

        // ▼▼▼ 핵심 수정 부분 ▼▼▼
        // 캐릭터가 다시 보이기 직전에, 위치를 WakeUpPoint로 강제 이동
        if (playerAction != null && wakeUpPoint != null)
        {
            playerAction.transform.position = wakeUpPoint.position;
        }
        // ▲▲▲ 핵심 수정 부분 ▲▲▲

        if (playerSprite != null)
        {
            playerSprite.enabled = true;
        }
        normalBedObject.SetActive(true);
        lyingDownBedObject.SetActive(false);

        if (playerFollowCamera != null && playerAction != null)
        {
            playerFollowCamera.Follow = playerAction.transform;
        }

        this.enabled = false;
    }
}