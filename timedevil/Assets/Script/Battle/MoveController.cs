using UnityEngine;
using UnityEngine.UI;

public class MoveController : MonoBehaviour
{
    [Header("UI")]
    public GameObject[] buttons;          // Card / Move / Item / Run 버튼

    [Header("Player")]
    public Transform playerTransform;     // Player_Stone
    public float tileSize = 1f;           // 한 칸 크기

    [Header("Board Bounds")]
    public Transform gridOrigin;          // 플레이어 보드 (0,0) 기준점
    public Vector2Int minGrid = new Vector2Int(0, 0);
    public Vector2Int maxGrid = new Vector2Int(3, 3);

    private bool isMoving = false;

    public void OnMoveButton()
    {
        if (!playerTransform || !gridOrigin) return;

        // 이동 모드 진입: 버튼 잠깐 비활성화
        SetButtonsActive(false);
        isMoving = true;
    }

    void Update()
    {
        if (!isMoving) return;

        if (Input.GetKeyDown(KeyCode.UpArrow)) TryMove(Vector2Int.up);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) TryMove(Vector2Int.down);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) TryMove(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) TryMove(Vector2Int.right);
    }

    // 현재 격자 좌표 계산
    Vector2Int GetCurrentGrid()
    {
        Vector3 local = playerTransform.position - gridOrigin.position;
        return new Vector2Int(
            Mathf.RoundToInt(local.x / tileSize),
            Mathf.RoundToInt(local.y / tileSize)
        );
    }

    void TryMove(Vector2Int dir)
    {
        if (!playerTransform || !gridOrigin) { ExitMoveMode(reEnableButtons: true, endTurn: false); return; }

        Vector2Int cur = GetCurrentGrid();
        Vector2Int target = cur + dir;

        // 🔒 범위 체크: 보드 밖이면 무시하고 버튼 복구(턴 소비 X)
        if (target.x < minGrid.x || target.x > maxGrid.x ||
            target.y < minGrid.y || target.y > maxGrid.y)
        {
            ExitMoveMode(reEnableButtons: true, endTurn: false);
            return;
        }

        // 월드 좌표로 변환 (Z 유지)
        Vector3 newPos = gridOrigin.position + new Vector3(target.x * tileSize, target.y * tileSize, 0f);
        newPos.z = playerTransform.position.z;
        playerTransform.position = newPos;

        Debug.Log($"🚶 [MoveController] 이동: {cur} → {target}");

        // 이동 1회로 종료, 버튼 복구 + 턴 종료
        ExitMoveMode(reEnableButtons: true, endTurn: true);
    }

    void ExitMoveMode(bool reEnableButtons, bool endTurn)
    {
        isMoving = false;
        if (reEnableButtons) SetButtonsActive(true);
        if (endTurn && TurnManager.Instance != null)
            TurnManager.Instance.EndPlayerTurn();
    }

    void SetButtonsActive(bool active)
    {
        if (buttons == null) return;
        foreach (var b in buttons)
            if (b) b.SetActive(active);
    }
}
