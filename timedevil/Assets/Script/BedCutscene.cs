using UnityEngine;
using System.Collections;
using Cinemachine; // Cinemachine�� ����ϱ� ���� �ʿ�

public class BedCutscene : MonoBehaviour, IInteractable
{
    [Header("1. �̺�Ʈ ������ ��ȭ")]
    public Dialogue[] dialogues; // ������� 4���� ��ȭ�� ���� �迭

    [Header("2. ī�޶� �� ȿ��")]
    public CinemachineVirtualCamera virtualCamera; // ������ '��' (Virtual Camera)
    public AudioClip thumpSound; // '�н�' ����
    public string nextSceneName; // �������� �Ѿ �� �̸�

    private int interactionCount = 0; // �� ��° ��ȣ�ۿ����� ����ϴ� ����
    private AudioSource audioSource;

    private void Start()
    {
        // ħ�� ������Ʈ�� �ִ� AudioSource�� ������
        audioSource = GetComponent<AudioSource>();
    }

    // PlayerAction ��ũ��Ʈ�� �� �Լ��� ȣ���մϴ�.
    public void Interact()
    {
        // ��ȣ�ۿ� Ƚ���� ���� �ٸ� �ൿ�� ����
        switch (interactionCount)
        {
            case 0: // ù ��° ��ȣ�ۿ�
                DialogueManager.instance.StartDialogue(dialogues[0]);
                interactionCount++; // ���� ��ȣ�ۿ��� ���� ī��Ʈ 1 ����
                break;
            case 1: // �� ��° ��ȣ�ۿ�
                StartCoroutine(FadeZoomDialogue(dialogues[1], 4.0f)); // ī�޶� ũ�� 4.5�� ����
                interactionCount++;
                break;
            case 2: // �� ��° ��ȣ�ۿ�
                StartCoroutine(FadeZoomDialogue(dialogues[2], 3.0f)); // �� ����
                interactionCount++;
                break;
            case 3: // �� ��° ��ȣ�ۿ�
                StartCoroutine(FadeZoomDialogue(dialogues[3], 2.0f)); // �� ����
                interactionCount++;
                break;
            case 4: // ������ ��ȣ�ۿ�
                StartCoroutine(FinalSequence());
                break;
        }
    }

    // ���̵�, ��, ��ȭ�� ������� ó���ϴ� ���
    IEnumerator FadeZoomDialogue(Dialogue dialogue, float targetZoomSize)
    {
        // 1. ȭ���� �˰� (���̵� �ƿ�)
        yield return StartCoroutine(SceneFader.instance.Fade(1f));

        // 2. ȭ���� ���� ���� '��'�� ���� ũ�⸦ ������ ����
        if (virtualCamera != null)
        {
            virtualCamera.m_Lens.OrthographicSize = targetZoomSize;
        }

        // 3. ��ȭ ����
        DialogueManager.instance.StartDialogue(dialogue);

        // 4. �ٽ� ȭ���� ��� (���̵� ��)
        yield return StartCoroutine(SceneFader.instance.Fade(0f));
    }

    // ������ ������(���� -> �Ҹ� -> �� ��ȯ)�� ó���ϴ� ���
    IEnumerator FinalSequence()
    {
        // 1. ������ ��Ӱ�
        yield return StartCoroutine(SceneFader.instance.Fade(1f));

        // 2. ȭ���� ���� ���� '�н�' �Ҹ� ���
        if (audioSource != null && thumpSound != null)
        {
            audioSource.PlayOneShot(thumpSound);
        }

        // 3. 1.5�� ���� ��ٸ�
        yield return new WaitForSeconds(1.5f);

        // 4. ���� ������ ��ȯ
        SceneFader.instance.LoadSceneWithFade(nextSceneName);
    }
}