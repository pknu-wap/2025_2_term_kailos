using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 필요

public class SceneStartDialogue : MonoBehaviour
{
    public Dialogue dialogue;

    [Tooltip("페이드인이 끝난 후, 독백이 시작되기까지의 대기 시간(초)")]
    public float delayBeforeStart = 0.5f; // 독백 시작 전 딜레이 시간

    // 이 오브젝트가 활성화될 때 호출됨
    private void OnEnable()
    {
        // SceneFader의 '페이드인 완료' 신호를 받으면 TriggerMonologue 함수를 실행하도록 등록
        SceneFader.OnFadeInComplete += TriggerMonologue;
    }

    // 이 오브젝트가 비활성화될 때 호출됨
    private void OnDisable()
    {
        // 등록을 해제하여 메모리 누수 방지
        SceneFader.OnFadeInComplete -= TriggerMonologue;
    }

    // '페이드인 완료' 신호를 받았을 때 코루틴을 시작시키는 역할
    void TriggerMonologue()
    {
        StartCoroutine(StartMonologueCoroutine());
    }

    // 실제 독백을 시작하는 코루틴
    IEnumerator StartMonologueCoroutine()
    {
        // Inspector에서 설정된 시간만큼 대기
        yield return new WaitForSeconds(delayBeforeStart);

        // 대화 시작
        DialogueManager.instance.StartDialogue(dialogue);

        // 독백을 한 번만 실행하고, 다시는 실행되지 않도록 이 스크립트 컴포넌트 자체를 비활성화
        this.enabled = false;
    }
}