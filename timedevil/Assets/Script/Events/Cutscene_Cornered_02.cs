using UnityEngine;
using System.Collections;

public class Cutscene_Cornered_02 : MonoBehaviour
{
    // ▼▼▼ [추가됨] 시작 전 대기 시간 ▼▼▼
    [Header("0. 시작 딜레이")]
    public float startDelay = 1.0f;

    [Header("1. 대상 연결")]
    public GameObject helper;
    public Transform helperExitPoint;

    [Header("2. 설정")]
    public float moveSpeed = 2.0f;
    public Dialogue dialogue;

    [Header("3. 카메라 이동 설정 (2번 카메라)")]
    public GameObject cutsceneCameraObject;
    public float cameraMoveDelay = 2.0f;

    public void StartPart2()
    {
        StartCoroutine(CutsceneSequence());
    }

    IEnumerator CutsceneSequence()
    {
        // ▼▼▼ 1. 시작 딜레이 (여기서 잠깐 멈췄다가 시작) ▼▼▼
        if (startDelay > 0)
        {
            yield return new WaitForSeconds(startDelay);
        }

        // 2. 컷씬용 2번 카메라 켜기 (부드러운 이동 시작)
        if (cutsceneCameraObject != null)
        {
            cutsceneCameraObject.SetActive(true);
        }

        // 카메라가 날아가는 시간 동안 대기
        yield return new WaitForSeconds(cameraMoveDelay);

        // 3. 작별 대화 시작
        if (DialogueManager.instance != null && dialogue.sentences.Length > 0)
        {
            DialogueManager.instance.StartDialogue(dialogue);
            yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);
        }

        // 4. 조력자 퇴장 이동
        if (helper != null && helperExitPoint != null)
        {
            Vector3 dir = (helperExitPoint.position - helper.transform.position).normalized;
            if (dir.x != 0)
            {
                Vector3 scale = helper.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (dir.x > 0 ? -1 : 1);
                helper.transform.localScale = scale;
            }

            while (Vector3.Distance(helper.transform.position, helperExitPoint.position) > 0.1f)
            {
                helper.transform.position = Vector3.MoveTowards(
                    helper.transform.position,
                    helperExitPoint.position,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        // 5. 조력자 사라짐
        if (helper != null)
        {
            helper.SetActive(false);
        }

        // 6. 카메라 복구 (1번 카메라로 돌아감)
        if (cutsceneCameraObject != null)
        {
            cutsceneCameraObject.SetActive(false);
            yield return new WaitForSeconds(1.5f); // 돌아오는 시간 대기
        }

        // 7. 모든 상황 종료!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isAction = false;
        }

        gameObject.SetActive(false);
    }
}