using UnityEngine;

/// <summary>
/// EnemyTurn 동안만 방향키 입력으로 한 칸 피했다가 되돌아오는 연출.
/// 실제 이동은 PlayerAnimeController가 처리.
/// </summary>
public class PlayerEvadeController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform playerStone;
    [SerializeField] private Transform gridOrigin;
    [SerializeField] private PlayerAnimeController anime;   // ★ 연결 필수

    [Header("Board")]
    [SerializeField] private Vector2Int minGrid = new Vector2Int(0, 0);
    [SerializeField] private Vector2Int maxGrid = new Vector2Int(3, 3);
    [SerializeField] private float tileSize = 1f;

    [Header("Evade")]
    [SerializeField] private float totalDuration = 0.6f;     // 왕복 총 시간
    [SerializeField] private float holdAtEdge = 0.05f;        // 끝에서 잠깐 유지
    [SerializeField] private bool lockInputDuringEvade = true;

    void Reset()
    {
        anime = GetComponent<PlayerAnimeController>();
    }

    void Update()
    {
        if (!playerStone || !gridOrigin || anime == null) return;

        // ✅ EnemyTurn일 때만 작동 (PlayerTurn에는 절대 동작 X)
        if (TurnManager.Instance == null ||
            TurnManager.Instance.currentTurn != TurnState.EnemyTurn)
            return;

        if (lockInputDuringEvade && anime.IsPlaying) return;

        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector2Int.right;

        if (dir == Vector2Int.zero) return;

        var cur = WorldToGrid(playerStone.position);
        var target = cur + dir;
        if (!InBounds(target)) return;

        float z = playerStone.position.z;
        var endPos = GridToWorld(target, z);

        // 왕복 애니메이션 (half = total/2)
        anime.AnimatePingPong(endPos, totalDuration * 0.5f, holdAtEdge, null);
    }

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
}
