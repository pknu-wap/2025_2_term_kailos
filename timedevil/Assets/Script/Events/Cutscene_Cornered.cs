using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class Cutscene_Cornered : MonoBehaviour
{
    [Header("1. 컷씬에 등장할 대상")]
    public PlayerAction player;
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

    // ▼▼▼ (수정) 사용하지 않는 monsterAnimator 변수 제거 ▼▼▼
    // private Animator monsterAnimator; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCutsceneRunning || (triggerOnce && isCutsceneRunning)) return;

        if (other.GetComponent<PlayerAction>() != null)
        {
            if (player == null || monster == null || helper == null || playerTargetPoint == null || monsterTargetPoint == null || helperSpawnPoint == null)
            {
                Debug.LogError("[Cutscene_Cornered] 컷씬에 필요한 오브젝트가 모두 연결되지 않았습니다! (인스펙터 확인)");
                return;
            }
            StartCoroutine(CutsceneCoroutine());
        }
    }

    IEnumerator CutsceneCoroutine()
    {
        isCutsceneRunning = true;

        // --- 1. 준비 단계: 조작 비활성화 및 컴포넌트 가져오기 ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isAction = true;
        }

        UndeadMover monsterMover = monster.GetComponent<UndeadMover>();
        if (monsterMover != null)
        {
            monsterMover.StopAllCoroutines();
            monsterMover.enabled = false;
        }

        // ▼▼▼ (수정) 사용하지 않는 monsterAnimator 찾기 코드 제거 ▼▼▼
        // monsterAnimator = monster.GetComponent<Animator>();

        // (페이드인/아웃 로직)
        yield return StartCoroutine(SceneFader.instance.Fade(1f));
        yield return StartCoroutine(SceneFader.instance.Fade(0f));

        // --- 2. 대화 1: "!" (예: dialogues[0]) ---
        if (dialogues.Length > 0)
        {
            DialogueManager.instance.StartDialogue(dialogues[0]);
            yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);
        }

        // --- 3. 동시 이동: 플레이어(뒷걸음질), 몬스터(접근) ---
        // (플레이어와 몬스터의 애니메이션 제어 코드가 모두 없는 상태)

        bool playerAtTarget = false;
        bool monsterAtTarget = false;

        while (!playerAtTarget || !monsterAtTarget)
        {
            // (이동 로직은 동일)
            if (!playerAtTarget)
            {
                player.transform.position = Vector3.MoveTowards(
                    player.transform.position, playerTargetPoint.position, playerMoveSpeed * Time.deltaTime
                );
                if (Vector3.Distance(player.transform.position, playerTargetPoint.position) < 0.01f)
                    playerAtTarget = true;
            }
            if (!monsterAtTarget)
            {
                monster.transform.position = Vector3.MoveTowards(
                    monster.transform.position, monsterTargetPoint.position, monsterMoveSpeed * Time.deltaTime
                );
                if (Vector3.Distance(monster.transform.position, monsterTargetPoint.position) < 0.01f)
                    monsterAtTarget = true;
            }
            yield return null;
        }

        // --- 4. 대화 2: "이제 끝이다..." (예: dialogues[1]) ---
        // (이하 컷씬 로직은 동일)
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