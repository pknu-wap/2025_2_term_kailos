using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class Cutscene_Cornered : MonoBehaviour
{
    // (인스펙터 변수들은 동일)
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
    [Header("4. 컷씬 중 대화 (순서대로)")]
    public Dialogue[] dialogues;
    [Header("5. 트리거 설정")]
    public bool triggerOnce = true;

    private bool isCutsceneRunning = false;
    private Animator fakePlayerAnimator; // 가짜 플레이어의 애니메이터

    private void OnTriggerEnter2D(Collider2D other)
    {
        // (이하 동일)
        if (isCutsceneRunning || (triggerOnce && isCutsceneRunning)) return;
        if (other.GetComponent<PlayerAction>() != null)
        {
            if (player == null || fakePlayerActor == null || monster == null || helper == null || playerTargetPoint == null || monsterTargetPoint == null || helperSpawnPoint == null)
            {
                Debug.LogError("[Cutscene_Cornered] 컷씬에 필요한 오브젝트가 모두 연결되지 않았습니다! (인스펙터 확인)");
                return;
            }
            StartCoroutine(CutsceneCoroutine());
        }
    }

    IEnumerator CutsceneCoroutine()
    {
        // (이하 준비 단계 동일)
        isCutsceneRunning = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isAction = true;
        }
        fakePlayerAnimator = fakePlayerActor.GetComponent<Animator>();
        player.gameObject.SetActive(false);
        fakePlayerActor.transform.position = player.transform.position;
        fakePlayerActor.transform.rotation = player.transform.rotation;
        fakePlayerActor.SetActive(true);
        UndeadMover monsterMover = monster.GetComponent<UndeadMover>();
        if (monsterMover != null)
        {
            monsterMover.StopAllCoroutines();
            monsterMover.enabled = false;
        }
        yield return StartCoroutine(SceneFader.instance.Fade(1f));
        yield return StartCoroutine(SceneFader.instance.Fade(0f));
        if (dialogues.Length > 0)
        {
            DialogueManager.instance.StartDialogue(dialogues[0]);
            yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);
        }

        // --- 3. 동시 이동 ---
        bool playerAtTarget = false;
        bool monsterAtTarget = false;

        while (!playerAtTarget || !monsterAtTarget)
        {
            if (!playerAtTarget)
            {
                // 3a. (가짜)플레이어 위치 이동
                fakePlayerActor.transform.position = Vector3.MoveTowards(
                    fakePlayerActor.transform.position, playerTargetPoint.position, playerMoveSpeed * Time.deltaTime
                );

                // ▼▼▼ [핵심 수정] PlayerAction의 애니메이션 로직을 정확히 흉내 냄 ▼▼▼
                if (fakePlayerAnimator != null)
                {
                    Vector2 moveDirection = (playerTargetPoint.position - fakePlayerActor.transform.position).normalized;
                    int h = 0;
                    int v = 0;

                    // PlayerAction의 isHorizonMove 로직 흉내
                    if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
                    {
                        h = (int)Mathf.Sign(moveDirection.x); // (1 또는 -1)
                    }
                    else
                    {
                        v = (int)Mathf.Sign(moveDirection.y); // (1 또는 -1)
                    }

                    // PlayerAction의 'isChange' 로직 흉내
                    if (fakePlayerAnimator.GetInteger("hAxisRaw") != h)
                    {
                        fakePlayerAnimator.SetBool("isChange", true);
                        fakePlayerAnimator.SetInteger("hAxisRaw", h);
                        fakePlayerAnimator.SetInteger("vAxisRaw", 0); // (v는 0으로 리셋)
                    }
                    else if (fakePlayerAnimator.GetInteger("vAxisRaw") != v)
                    {
                        fakePlayerAnimator.SetBool("isChange", true);
                        fakePlayerAnimator.SetInteger("hAxisRaw", 0); // (h는 0으로 리셋)
                        fakePlayerAnimator.SetInteger("vAxisRaw", v);
                    }
                    else
                    {
                        // (중요!) 방향이 바뀌지 않았으므로 isChange를 false로 돌림
                        // (이래야 Any State가 애니메이션을 재시작하지 않음)
                        fakePlayerAnimator.SetBool("isChange", false);
                    }
                }
                // ▲▲▲ [핵심 수정 끝] ▲▲▲

                // 3c. (도착 체크)
                if (Vector3.Distance(fakePlayerActor.transform.position, playerTargetPoint.position) < 0.01f)
                    playerAtTarget = true;
            }

            if (!monsterAtTarget)
            {
                // (몬스터 이동은 슬라이딩 방식)
                monster.transform.position = Vector3.MoveTowards(
                    monster.transform.position, monsterTargetPoint.position, monsterMoveSpeed * Time.deltaTime
                );
                if (Vector3.Distance(monster.transform.position, monsterTargetPoint.position) < 0.01f)
                    monsterAtTarget = true;
            }
            yield return null; // 다음 프레임까지 대기
        }

        // 3d. (가짜)플레이어 걷기 애니메이션 끄기 (도착 완료)
        if (fakePlayerAnimator != null)
        {
            fakePlayerAnimator.SetBool("isChange", false);
            fakePlayerAnimator.SetInteger("hAxisRaw", 0);
            fakePlayerAnimator.SetInteger("vAxisRaw", 0);
        }

        // (이하 컷씬 로직은 동일)
        // ... (대화 2, 헬퍼 등장, 대화 3, 몬스터 제거, 대화 4, 컷씬 종료) ...
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
        yield return new WaitForSeconds(1.0f);
        monster.SetActive(false);
        if (dialogues.Length > 3)
        {
            DialogueManager.instance.StartDialogue(dialogues[3]);
            yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);
        }
        fakePlayerActor.SetActive(false);
        player.transform.position = fakePlayerActor.transform.position;
        player.gameObject.SetActive(true);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isAction = false;
        }
        if (triggerOnce)
        {
            gameObject.SetActive(false);
        }
        isCutsceneRunning = false;
    }
}