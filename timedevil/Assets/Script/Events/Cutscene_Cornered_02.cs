using UnityEngine;
using System.Collections;
using Cinemachine;

public class Cutscene_Cornered_02 : MonoBehaviour
{
    [Header("0. 시작 딜레이")]
    public float startDelay = 0.5f;

    [Header("1. 대상 연결")]
    public GameObject helper;
    public Transform[] movePath;

    [Header("2. 설정")]
    public float moveSpeed = 2.0f;
    public Dialogue dialogue;

    [Header("3. 카메라 이동 설정 (2번 카메라)")]
    public GameObject cutsceneCameraObject;
    public float cameraMoveDelay = 2.0f;

    // ▼▼▼ [추가됨] 교체할 스프라이트 이미지들 ▼▼▼
    [Header("4. 스프라이트 수동 교체")]
    public Sprite playerIdleRight; // 플레이어가 오른쪽 보는 멈춘 이미지
    public Sprite helperIdleLeft;  // 조력자가 왼쪽 보는 멈춘 이미지

    public void StartPart2()
    {
        StartCoroutine(CutsceneSequence());
    }

    IEnumerator CutsceneSequence()
    {
        if (cutsceneCameraObject != null) cutsceneCameraObject.SetActive(true);
        if (startDelay > 0) yield return new WaitForSeconds(startDelay);
        if (cameraMoveDelay > 0) yield return new WaitForSeconds(cameraMoveDelay);

        // ▼▼▼ [핵심 수정] 대화 전, 애니메이터 끄고 스프라이트 교체 ▼▼▼

        // (1) 조력자 처리
        if (helper != null)
        {
            Animator helperAnim = helper.GetComponent<Animator>();
            SpriteRenderer helperSR = helper.GetComponent<SpriteRenderer>();

            // 애니메이터가 방해하지 못하게 끕니다.
            if (helperAnim != null) helperAnim.enabled = false;

            // 준비한 '왼쪽 보기' 스프라이트로 교체합니다.
            if (helperSR != null && helperIdleLeft != null)
            {
                helperSR.sprite = helperIdleLeft;
            }
        }

        // (2) 실제 플레이어 처리
        PlayerAction realPlayer = FindObjectOfType<PlayerAction>();
        if (realPlayer != null)
        {
            Animator playerAnim = realPlayer.GetComponent<Animator>();
            SpriteRenderer playerSR = realPlayer.GetComponent<SpriteRenderer>();

            // 애니메이터 끄기
            if (playerAnim != null) playerAnim.enabled = false;

            // '오른쪽 보기' 스프라이트로 교체
            if (playerSR != null && playerIdleRight != null)
            {
                playerSR.sprite = playerIdleRight;
            }
        }
        // ▲▲▲▲▲▲

        if (DialogueManager.instance != null && dialogue.sentences.Length > 0)
        {
            DialogueManager.instance.StartDialogue(dialogue);
            yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);
        }

        // ▼▼▼ 조력자 이동 (이동할 땐 애니메이터 다시 켜기) ▼▼▼
        if (helper != null && movePath != null && movePath.Length > 0)
        {
            Animator helperAnim = helper.GetComponent<Animator>();
            SpriteRenderer helperSR = helper.GetComponent<SpriteRenderer>();

            // 이동 시작 전 애니메이터 다시 활성화!
            if (helperAnim != null) helperAnim.enabled = true;

            foreach (Transform targetPoint in movePath)
            {
                if (targetPoint == null) continue;
                while (Vector3.Distance(helper.transform.position, targetPoint.position) > 0.01f)
                {
                    helper.transform.position = Vector3.MoveTowards(
                        helper.transform.position, targetPoint.position, moveSpeed * Time.deltaTime
                    );

                    // (기존 애니메이션 방향 로직 유지)
                    if (helperAnim != null)
                    {
                        Vector3 dir = (targetPoint.position - helper.transform.position).normalized;
                        int h = 0; int v = 0;
                        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) h = (int)Mathf.Sign(dir.x); else v = (int)Mathf.Sign(dir.y);
                        if (helperAnim.GetInteger("hAxisRaw") != h) { helperAnim.SetBool("isChange", true); helperAnim.SetInteger("hAxisRaw", h); helperAnim.SetInteger("vAxisRaw", 0); }
                        else if (helperAnim.GetInteger("vAxisRaw") != v) { helperAnim.SetBool("isChange", true); helperAnim.SetInteger("hAxisRaw", 0); helperAnim.SetInteger("vAxisRaw", v); }
                        else helperAnim.SetBool("isChange", false);
                    }
                    yield return null;
                }
            }

            // ▼▼▼ 도착 후, 다시 애니메이터 끄고 스프라이트 고정 ▼▼▼
            if (helperAnim != null) helperAnim.enabled = false;
            if (helperSR != null && helperIdleLeft != null)
            {
                helperSR.sprite = helperIdleLeft; // 마지막 모습은 왼쪽 보기
            }
        }

        if (helper != null) helper.SetActive(false);
        if (cutsceneCameraObject != null) { cutsceneCameraObject.SetActive(false); yield return new WaitForSeconds(1.5f); }

        // (중요) 플레이어 애니메이터는 게임 재개 전에 다시 켜줘야 합니다.
        if (realPlayer != null)
        {
            Animator playerAnim = realPlayer.GetComponent<Animator>();
            if (playerAnim != null) playerAnim.enabled = true;
        }

        if (GameManager.Instance != null) GameManager.Instance.isAction = false;
        gameObject.SetActive(false);
    }
}