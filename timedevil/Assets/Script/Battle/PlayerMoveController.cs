using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject[] buttons;      // Card/Move/Item/Run

    [Header("Refs")]
    [SerializeField] private Transform playerTransform; // Player_Stone
    [SerializeField] private Transform gridOrigin;
    [SerializeField] private PlayerAnimeController anime; // ★ 연결 필수

    [Header("Board")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private Vector2Int minGrid = new Vector2Int(0, 0);
    [SerializeField] private Vector2Int maxGrid = new Vector2Int(3, 3);

    [Header("Move Anim")]
    [SerializeField] private float moveDuration = 0.18f;

    bool isMoveMode = false;

    void Reset()
    {
        anime = GetComponent<PlayerAnimeController>();
    }

    public void OnMoveButton()
    {
        if (!playerTransform || !gridOrigin || anime == null) return;

        // 🔒 PlayerTurn일 때만 이동 모드 진입
        if (TurnManager.Instance == null || TurnManager.Instance.currentTurn != TurnState.PlayerTurn)
            return;

        SetButtons(false);
        isMoveMode = true;
    }

    void Update()
    {
        if (TurnManager.Instance == null || TurnManager.Instance.currentTurn != TurnState.PlayerTurn)
            return;

        if (!isMoveMode || anime == null || anime.IsPlaying) return;

        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector2Int.right;

        if (dir == Vector2Int.zero) return;

        var cur = WorldToGrid(playerTransform.position);
        var target = cur + dir;

        // 범위 밖 → 이동 실패(턴 소모 없음), 버튼만 복구
        if (!InBounds(target))
        {
            ExitMoveMode(reEnableButtons: true, endTurn: false);
            return;
        }

        // 목표 월드좌표
        float z = playerTransform.position.z;
        var endPos = GridToWorld(target, z);

        // 애니메이션으로 "딱 한 칸" 이동
        anime.AnimateTo(endPos, moveDuration);

        // 애니메이션 끝난 후 턴 종료 시점 동기화
        StartCoroutine(Co_EndAfterAnim());
    }

    System.Collections.IEnumerator Co_EndAfterAnim()
    {
        // 애니 끝날 때까지 대기
        while (anime.IsPlaying) yield return null;
        ExitMoveMode(reEnableButtons: true, endTurn: true);
    }

    // ---------- helpers ----------
    Vector2Int WorldToGrid(Vector3 world)
    {
        Vector3 local = world - gridOrigin.position;
        return new Vector2Int(
            Mathf.RoundToInt(local.x / tileSize),
            Mathf.RoundToInt(local.y / tileSize)
        );
    }

    bool InBounds(Vector2Int g) =>
        (g.x >= minGrid.x && g.x <= maxGrid.x &&
         g.y >= minGrid.y && g.y <= maxGrid.y);

    Vector3 GridToWorld(Vector2Int g, float keepZ)
    {
        var p = gridOrigin.position + new Vector3(g.x * tileSize, g.y * tileSize, 0f);
        p.z = keepZ;
        return p;
    }

    void ExitMoveMode(bool reEnableButtons, bool endTurn)
    {
        isMoveMode = false;
        if (reEnableButtons) SetButtons(true);
        if (endTurn && TurnManager.Instance != null)
            TurnManager.Instance.EndPlayerTurn();
    }

    void SetButtons(bool on)
    {
        if (buttons == null) return;
        foreach (var b in buttons) if (b) b.SetActive(on);
    }
}
