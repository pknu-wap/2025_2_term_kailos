using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    public Dialogue dialogue;

    public void Interact()
    {
        // ���� Ÿ���� ��ġ�ϹǷ� ���������� ȣ��˴ϴ�.
        DialogueManager.instance.StartDialogue(dialogue);

        // ���̾ Ȯ���Ͽ� ������ ȹ�� ������ �����մϴ�.
        if (gameObject.layer == LayerMask.NameToLayer("item_get"))
        {
            Debug.Log("�������� ȹ���߽��ϴ�!");
            // ������ ȹ�� �� ��ȣ�ۿ� ��Ȱ��ȭ ���� ó���� ���⿡ �߰�...
        }
    }
}