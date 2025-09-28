using UnityEngine;

public class SceneStartDialogue : MonoBehaviour
{
    // Inspector 창에서 입력할 독백 내용을 담을 변수
    public Dialogue dialogue;

    // 이 스크립트가 활성화될 때 (씬이 시작될 때) 자동으로 한 번만 실행되는 함수
    void Start()
    {
        // DialogueManager를 찾아 독백 시작을 요청
        DialogueManager.instance.StartDialogue(dialogue);
    }
}