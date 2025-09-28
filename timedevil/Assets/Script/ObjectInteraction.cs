using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    public Dialogue dialogue;

    public void Interact()
    {
        // 이제 타입이 일치하므로 정상적으로 호출됩니다.
        DialogueManager.instance.StartDialogue(dialogue);

        // 레이어를 확인하여 아이템 획득 로직을 실행합니다.
        if (gameObject.layer == LayerMask.NameToLayer("item_get"))
        {
            Debug.Log("아이템을 획득했습니다!");
            // 아이템 획득 후 상호작용 비활성화 등의 처리를 여기에 추가...
        }
    }
}