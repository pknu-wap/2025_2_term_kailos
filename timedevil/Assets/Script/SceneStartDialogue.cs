using UnityEngine;

public class SceneStartDialogue : MonoBehaviour
{
    // Inspector â���� �Է��� ���� ������ ���� ����
    public Dialogue dialogue;

    // �� ��ũ��Ʈ�� Ȱ��ȭ�� �� (���� ���۵� ��) �ڵ����� �� ���� ����Ǵ� �Լ�
    void Start()
    {
        // DialogueManager�� ã�� ���� ������ ��û
        DialogueManager.instance.StartDialogue(dialogue);
    }
}