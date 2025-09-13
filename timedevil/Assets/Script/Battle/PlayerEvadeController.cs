// PlayerEvadeController.cs
using System.Collections;
using UnityEngine;

public class PlayerEvadeController : MonoBehaviour
{
    [Header("Player (왼쪽 보드)")]
    [SerializeField] private Transform playerStone;  // Player_Stone
    [SerializeField] private Transform gridOrigin;   // 왼쪽 보드 (0,0) 칸의 중심
    [SerializeField] private Vector2Int minGrid = new Vector2Int(0, 0);
    [SerializeField] private Vector2Int maxGrid = new Vector2Int(3, 3);
    [SerializeField] private float tileSize = 1f;

    [Header("Evade 설정")]
    [SerializeField] private float evadeDuration = 0.6f; // n초
    [SerializeField] private bool lockInputDuringEvade = true;

    private bool isEvading = false;
    private Vector3 originalPos;

    void Update()
    {
        if (playerStone == null || gridOrigin == null) return;

        // 적 턴일 때만 동작
        if (TurnManager.Instance == null || TurnManager.Instance.currentTurn != TurnState.EnemyTurn)
            return;

        if (isEvading && lockInputDuringEvade) return;

        // 방향 입력
        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.UpArrow)) dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector2Int.right;

        if (dir == Vector2Int.zero) return;

        TryEvade(dir);
    }

    void TryEvade(Vector2Int dir)
    {
        // 현재 그리드 좌표 계산
        Vector3 origin = gridOrigin.position;
        Vector3 local = playerStone.position - origin;
        Vector2Int grid = new Vector2Int(
            Mathf.RoundToInt(local.x / tileSize),
            Mathf.RoundToInt(local.y / tileSize)
        );

        Vector2Int target = grid + dir;

        // 범위 체크 (범위 밖이면 무시)
        if (target.x < minGrid.x || target.x > maxGrid.x || target.y < minGrid.y || target.y > maxGrid.y)
            return;

        // 이동 및 n초 후 복귀
        StartCoroutine(Co_Evade(target, origin));
    }

    IEnumerator Co_Evade(Vector2Int targetGrid, Vector3 origin)
    {
        isEvading = true;
        originalPos = playerStone.position;

        // 타겟 위치 (Z 유지)
        Vector3 targetPos = origin + new Vector3(targetGrid.x * tileSize, targetGrid.y * tileSize, 0f);
        targetPos.z = originalPos.z;

        playerStone.position = targetPos;

        // n초 동안 추가 입력 잠금
        yield return new WaitForSeconds(evadeDuration);

        // 원위치 복귀
        playerStone.position = originalPos;

        isEvading = false;
        // 턴 흐름은 TurnManager(적 공격 연출 종료 시 EndEnemyTurn)에서 그대로 진행
    }
}
