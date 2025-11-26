using UnityEngine;
using System.Collections;
using Cinemachine; // 시네머신 필수

[RequireComponent(typeof(BoxCollider2D))]
public class Cutscene_Cornered : MonoBehaviour
{
    [Header("1. 컷씬에 등장할 대상")]
    public PlayerAction player;
    public GameObject fakePlayerActor;
    public GameObject monster;
    public GameObject helper;

    [Header("2. 이동할 목표 지점")]
    public Transform playerTargetPoint;
    public Transform monsterTargetPoint;
    public Transform helperSpawnPoint;

    [Header("3. 이동 속도 설정")]
    public float playerMoveSpeed = 2f;
    public float monsterMoveSpeed = 1.5f;

    [Header("4. 컷씬 중 대화")]
    public Dialogue[] dialogues;

    [Header("5. 다음 컷씬 연결")]
    public Cutscene_Cornered_02 nextCutscenePart;

    [Header("6. 스킬 및 사망 연출")]
    public GameObject skillEffectOnScene;
    public float deathDelay = 1.5f;

    [Header("7. 카메라 연출 (시네머신)")]
    public CinemachineVirtualCamera virtualCamera;
    public float targetCamSize = 3.5f; // 바꿀 카메라 크기

    private bool isCutsceneRunning = false;
    private Animator fakePlayerAnimator;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCutsceneRunning) return;

        if (other.GetComponent<PlayerAction>() != null)
        {
            if (player == null || fakePlayerActor == null || monster == null || helper == null ||
                playerTargetPoint == null || monsterTargetPoint == null || helperSpawnPoint == null)
            {
                Debug.LogError("[Cutscene_Cornered] 필수 오브젝트 연결 확인!");
                return;
            }
            if (nextCutscenePart == null) Debug.LogError("다음 컷씬(Part 2) 연결 안됨!");
            if (virtualCamera == null)
            {
                Debug.LogError("[Cutscene_Cornered] 시네머신 버추얼 카메라 연결 안됨!");
                return;
            }

            StartCoroutine(CutsceneCoroutine());
        }
    }

    IEnumerator CutsceneCoroutine()
    {
        isCutsceneRunning = true;

        if (GameManager.Instance != null) GameManager.Instance.isAction = true;

        fakePlayerAnimator = fakePlayerActor.GetComponent<Animator>();
        player.gameObject.SetActive(false);
        fakePlayerActor.transform.position = player.transform.position;
        fakePlayerActor.transform.rotation = player.transform.rotation;
        fakePlayerActor.SetActive(true);

        UndeadMover monsterMover = monster.GetComponent<UndeadMover>();
        if (monsterMover != null) { monsterMover.StopAllCoroutines(); monsterMover.enabled = false; }

        // 페이드 중에 카메라 크기 '탁' 바꾸기 (몰래 변경)
        if (SceneFader.instance != null)
        {
            yield return StartCoroutine(SceneFader.instance.Fade(1f));

            if (virtualCamera != null)
            {
                virtualCamera.m_Lens.OrthographicSize = targetCamSize;
            }

            yield return StartCoroutine(SceneFader.instance.Fade(0f));
        }

        if (dialogues.Length > 0)
        {
            DialogueManager.instance.StartDialogue(dialogues[0]);
            yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);
        }

        // --- 이동 로직 ---
        bool playerAtTarget = false;
        bool monsterAtTarget = false;

        while (!playerAtTarget || !monsterAtTarget)
        {
            if (!playerAtTarget)
            {
                fakePlayerActor.transform.position = Vector3.MoveTowards(
                    fakePlayerActor.transform.position, playerTargetPoint.position, playerMoveSpeed * Time.deltaTime
                );

                if (fakePlayerAnimator != null)
                {
                    Vector2 dir = (playerTargetPoint.position - fakePlayerActor.transform.position).normalized;
                    int h = 0, v = 0;
                    if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) h = (int)Mathf.Sign(dir.x);
                    else v = (int)Mathf.Sign(dir.y);

                    if (fakePlayerAnimator.GetInteger("hAxisRaw") != h) { fakePlayerAnimator.SetBool("isChange", true); fakePlayerAnimator.SetInteger("hAxisRaw", h); fakePlayerAnimator.SetInteger("vAxisRaw", 0); }
                    else if (fakePlayerAnimator.GetInteger("vAxisRaw") != v) { fakePlayerAnimator.SetBool("isChange", true); fakePlayerAnimator.SetInteger("hAxisRaw", 0); fakePlayerAnimator.SetInteger("vAxisRaw", v); }
                    else fakePlayerAnimator.SetBool("isChange", false);
                }

                if (Vector3.Distance(fakePlayerActor.transform.position, playerTargetPoint.position) < 0.01f) playerAtTarget = true;
            }

            if (!monsterAtTarget)
            {
                monster.transform.position = Vector3.MoveTowards(monster.transform.position, monsterTargetPoint.position, monsterMoveSpeed * Time.deltaTime);
                if (Vector3.Distance(monster.transform.position, monsterTargetPoint.position) < 0.01f) monsterAtTarget = true;
            }
            yield return null;
        }

        if (fakePlayerAnimator != null) { fakePlayerAnimator.SetBool("isChange", false); fakePlayerAnimator.SetInteger("hAxisRaw", 0); fakePlayerAnimator.SetInteger("vAxisRaw", 0); }

        if (dialogues.Length > 1)
        {
            DialogueManager.instance.StartDialogue(dialogues[1]);
            yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);
        }

        helper.transform.position = helperSpawnPoint.position;
        helper.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        if (dialogues.Length > 2)
        {
            DialogueManager.instance.StartDialogue(dialogues[2]);
            yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);
        }

        if (skillEffectOnScene != null) skillEffectOnScene.SetActive(true);
        Animator monsterAnim = monster.GetComponent<Animator>();
        if (monsterAnim != null) monsterAnim.SetTrigger("doDie");

        yield return new WaitForSeconds(deathDelay);
        monster.SetActive(false);
        if (skillEffectOnScene != null) skillEffectOnScene.SetActive(false);

        if (dialogues.Length > 3)
        {
            DialogueManager.instance.StartDialogue(dialogues[3]);
            yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);
        }

        // ★★★ 여기 있던 "카메라 복구(Zoom Out)" 코드를 전부 삭제했습니다. ★★★

        // 1부 종료 및 인계
        fakePlayerActor.SetActive(false);
        player.transform.position = fakePlayerActor.transform.position;
        player.gameObject.SetActive(true);

        if (nextCutscenePart != null)
        {
            nextCutscenePart.StartPart2();
        }
        else
        {
            if (GameManager.Instance != null) GameManager.Instance.isAction = false;
            gameObject.SetActive(false);
        }
    }
}