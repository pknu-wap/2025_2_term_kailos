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

    [Header("플레이어 설정")]
    [Tooltip("컷신이 끝난 후 플레이어가 나타날 위치")]
    public Transform wakeUpPoint;

    [Header("연출 시간 설정")]
    public float delayBeforeSleep = 2f;
    public float delayBeforeWakeUp = 2f;

    private SpriteRenderer playerSprite;
    private PlayerAction playerAction;

    void Start()
    {
        if (PlayerReturnContext.HasReturnPosition)
        {
            this.enabled = false;
            return;
        }

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
        DialogueManager.instance.StartDialogue(openingMonologue);
        yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);
        yield return new WaitForSeconds(delayBeforeSleep);
        yield return StartCoroutine(SceneFader.instance.Fade(1f));
        yield return new WaitForSeconds(delayBeforeWakeUp);
        yield return StartCoroutine(SceneFader.instance.Fade(0f));

        if (playerAction != null && wakeUpPoint != null)
        {
            playerAction.transform.position = wakeUpPoint.position;
        }

        if (playerSprite != null)
        {
            playerSprite.enabled = true;
        }
        normalBedObject.SetActive(true);
        lyingDownBedObject.SetActive(false);

        this.enabled = false;
    }
}