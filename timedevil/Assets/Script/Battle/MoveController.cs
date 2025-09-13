using UnityEngine;
using UnityEngine.UI;

public class MoveController : MonoBehaviour
{
    public GameObject[] buttons;          // Card, Move, Item, Run 버튼들
    public Transform playerTransform;     // Player_Stone의 Transform
    public float tileSize = 1.0f;         // 한 칸 크기

    private bool isMoving = false;

    public void OnMoveButton()
    {
        Debug.Log("🚶 [MoveController] 이동 모드 시작");

        // 버튼 비활성화
        foreach (var btn in buttons)
            btn.SetActive(false);

        isMoving = true;
    }

    void Update()
    {
        if (!isMoving) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            MovePlayer(Vector2Int.up);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            MovePlayer(Vector2Int.down);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            MovePlayer(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            MovePlayer(Vector2Int.right);
    }

    void MovePlayer(Vector2Int dir)
    {
        Vector3 newPos = playerTransform.position + new Vector3(dir.x * tileSize, dir.y * tileSize, 0);
        playerTransform.position = newPos;

        Debug.Log($"🚶 [MoveController] {dir} 방향으로 이동 완료");

        isMoving = false;

        // 턴 종료
        TurnManager.Instance.EndPlayerTurn();

        // 버튼 다시 활성화
        foreach (var btn in buttons)
            btn.SetActive(true);
    }
}
