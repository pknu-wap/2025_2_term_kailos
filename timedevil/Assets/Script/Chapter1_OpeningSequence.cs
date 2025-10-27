using UnityEngine;
using System.Collections;
using Cinemachine;

public class Chapter1_OpeningSequence : MonoBehaviour
{
    [Header("������ ����")]
    public Dialogue openingMonologue;

    [Header("ħ�� ������Ʈ ����")]
    public GameObject normalBedObject;
    public GameObject lyingDownBedObject;

    [Header("ī�޶� ����")]
    public CinemachineVirtualCamera playerFollowCamera;

    // ���� ���⿡ '�Ͼ�� ����' ���� �߰� ����
    [Header("�÷��̾� ����")]
    [Tooltip("�ƽ��� ���� �� �÷��̾ ��Ÿ�� ��ġ")]
    public Transform wakeUpPoint;
    // ����

    [Header("���� �ð� ����")]
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
        // ... (��ȭ, ���̵���/�ƿ� �� ���� ������ �״��) ...
        DialogueManager.instance.StartDialogue(openingMonologue);
        yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);
        yield return new WaitForSeconds(delayBeforeSleep);
        yield return StartCoroutine(SceneFader.instance.Fade(1f));
        yield return new WaitForSeconds(delayBeforeWakeUp);
        yield return StartCoroutine(SceneFader.instance.Fade(0f));

        // ���� �ٽ� ���� �κ� ����
        // ĳ���Ͱ� �ٽ� ���̱� ������, ��ġ�� WakeUpPoint�� ���� �̵�
        if (playerAction != null && wakeUpPoint != null)
        {
            playerAction.transform.position = wakeUpPoint.position;
        }
        // ���� �ٽ� ���� �κ� ����

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