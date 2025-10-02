using UnityEngine;
using System.Collections; // �ڷ�ƾ�� ����ϱ� ���� �ʿ�

public class SceneStartDialogue : MonoBehaviour
{
    public Dialogue dialogue;

    [Tooltip("���̵����� ���� ��, ������ ���۵Ǳ������ ��� �ð�(��)")]
    public float delayBeforeStart = 0.5f; // ���� ���� �� ������ �ð�

    // �� ������Ʈ�� Ȱ��ȭ�� �� ȣ���
    private void OnEnable()
    {
        // SceneFader�� '���̵��� �Ϸ�' ��ȣ�� ������ TriggerMonologue �Լ��� �����ϵ��� ���
        SceneFader.OnFadeInComplete += TriggerMonologue;
    }

    // �� ������Ʈ�� ��Ȱ��ȭ�� �� ȣ���
    private void OnDisable()
    {
        // ����� �����Ͽ� �޸� ���� ����
        SceneFader.OnFadeInComplete -= TriggerMonologue;
    }

    // '���̵��� �Ϸ�' ��ȣ�� �޾��� �� �ڷ�ƾ�� ���۽�Ű�� ����
    void TriggerMonologue()
    {
        StartCoroutine(StartMonologueCoroutine());
    }

    // ���� ������ �����ϴ� �ڷ�ƾ
    IEnumerator StartMonologueCoroutine()
    {
        // Inspector���� ������ �ð���ŭ ���
        yield return new WaitForSeconds(delayBeforeStart);

        // ��ȭ ����
        DialogueManager.instance.StartDialogue(dialogue);

        // ������ �� ���� �����ϰ�, �ٽô� ������� �ʵ��� �� ��ũ��Ʈ ������Ʈ ��ü�� ��Ȱ��ȭ
        this.enabled = false;
    }
}