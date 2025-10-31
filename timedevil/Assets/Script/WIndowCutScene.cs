using UnityEngine;
using System.Collections;

// �� ��ũ��Ʈ�� AudioSource ������Ʈ�� �ʼ��� �䱸�մϴ�.
[RequireComponent(typeof(AudioSource))]
public class WindowCutScene: MonoBehaviour
{
    [Header("1. â������ �� ��ȭ")]
    public Dialogue windowDialogue;

    [Space(10)]
    [Header("2. ������ �Ͼ �̺�Ʈ")]
    [Tooltip("â�� ��ȭ�� ������ �� �� �Ŀ� ���� �ε帱�� �����մϴ�.")]
    public float delayBeforeKnock = 2.0f;
    public AudioClip knockSound;
    public Dialogue doorDialogue;

    private AudioSource audioSource;
    private bool isSequenceRunning = false; // �ƾ��� �ߺ� ����Ǵ� ���� ����

    void Start()
    {
        // '�ȶȶ�' �Ҹ��� ����� AudioSource�� �����ɴϴ�.
        audioSource = GetComponent<AudioSource>();
    }

    // ���� �� ������Ʈ�� 'Ŭ��'�ϰų� '��ȣ�ۿ�'���� �� ȣ���� �Լ��Դϴ� ����
    // (���� �÷��̾ �ͼ� 'E'Ű�� ������ ����̶��, 
    //  �÷��̾� ��ũ��Ʈ���� �� �Լ��� ȣ���ϰ� �ؾ� �մϴ�.)

    // ������ �׽�Ʈ�ϱ� ������ '���콺 Ŭ��'���� �۵��ϰ� �صΰڽ��ϴ�.
    private void OnMouseDown()
    {
        // 1. �ƾ��� �̹� ���� ���̸� �ƹ��͵� �� ��
        if (isSequenceRunning) return;

        // 2. �ٸ� ��ȭ�� �̹� ���� ���̸� �ƹ��͵� �� ��
        if (DialogueManager.instance.isDialogueActive) return;

        // 3. ��� ������ ������ �ƾ� �ڷ�ƾ ����!
        StartCoroutine(FullSequence());
    }


    // ���� ��ü �̺�Ʈ ����(������)�� �����ϴ� �ڷ�ƾ ����
    IEnumerator FullSequence()
    {
        isSequenceRunning = true; // �ƾ� ���� �÷��� ON

        // --- 1. â�� ��ȭ ���� ---
        DialogueManager.instance.StartDialogue(windowDialogue);

        // --- 2. â�� ��ȭ�� ���� ������ ��� ---
        // DialogueManager�� isDialogueActive�� false�� �� ������ �� ������ ��ٸ��ϴ�.
        yield return new WaitUntil(() => !DialogueManager.instance.isDialogueActive);

        // --- 3. ������ �ð�(2��)��ŭ ��� ---
        yield return new WaitForSeconds(delayBeforeKnock);

        // --- 4. '�ȶȶ�' �Ҹ� ��� ---
        if (knockSound != null)
        {
            // 3D ���尡 �ƴ� 2D ����� ����Ϸ��� audioSource.PlayOneShot(knockSound) ���
            // BGM �Ŵ����� ���� �Ŵ����� �̱��� �ν��Ͻ��� ���� ����ϴ� ���� �� �����ϴ�.
            // ��: SoundManager.instance.PlaySFX(knockSound);
            audioSource.PlayOneShot(knockSound);
        }

        // --- 5. �� ��ȭ ���� ---
        DialogueManager.instance.StartDialogue(doorDialogue);

        // --- 6. �ƾ� ���� ---
        isSequenceRunning = false;

        // (���� ����) �� �̺�Ʈ�� �� �� ���� �����ϰ� �ʹٸ�,
        // �� ��ũ��Ʈ(�Ǵ� ������Ʈ)�� ��Ȱ��ȭ�մϴ�.
        // this.enabled = false; 
        // gameObject.SetActive(false);
    }
}