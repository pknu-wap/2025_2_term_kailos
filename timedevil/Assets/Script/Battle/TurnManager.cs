using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public enum TurnState { PlayerTurn, EnemyTurn }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public TurnState currentTurn;

    [Header("UI Buttons (플레이어 턴 활성/비활성)")]
    public Button cardBtn; public Button moveBtn; public Button itemBtn; public Button runBtn;

    [Header("Enemy 이동 세팅")]
    public Transform enemyStone;     // Enemy쪽 말(스톤) Transform
    public float tileSize = 1f;      // 칸 크기
    public Vector2Int minGrid = new Vector2Int(0,0);  // 이동 허용 좌표 최소
    public Vector2Int maxGrid = new Vector2Int(3,3);  // 이동 허용 좌표 최대
    public Transform gridOrigin;     // (0,0)의 월드 기준점 (선택)

    [Header("Refs")]
    public AttackController attackController;

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    void Start()
    {
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        SetButtons(true);
        Debug.Log("🔷 플레이어 턴 시작");
    }

    public void EndPlayerTurn()
    {
        SetButtons(false);
        StartEnemyTurn();
    }

    void SetButtons(bool on)
    {
        if (cardBtn) cardBtn.interactable = on;
        if (moveBtn) moveBtn.interactable = on;
        if (itemBtn) itemBtn.interactable = on;
        if (runBtn) runBtn.interactable = on;
    }

    public void StartEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("🔶 적 턴 시작");

        // 50% 이동 / 50% 공격
        if (UnityEngine.Random.value < 0.5f) EnemyMove();
        else EnemyAttack();
    }

void EnemyMove()
{
    if (!enemyStone) { EndEnemyTurn(); return; }

    // 기준점/그리드 좌표 계산
    Vector3 origin = gridOrigin ? gridOrigin.position : Vector3.zero;
    Vector3 local = enemyStone.position - origin;
    Vector2Int grid = new Vector2Int(
        Mathf.RoundToInt(local.x / tileSize),
        Mathf.RoundToInt(local.y / tileSize)
    );

    // 상하좌우 후보 중 경계 내 칸만
    Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    var candidates = dirs
        .Select(d => grid + d)
        .Where(g => g.x >= minGrid.x && g.x <= maxGrid.x && g.y >= minGrid.y && g.y <= maxGrid.y)
        .ToList();

    if (candidates.Count == 0) { EndEnemyTurn(); return; }

    var target = candidates[UnityEngine.Random.Range(0, candidates.Count)];

    // 이동할 월드 좌표 계산 + 🔒 Z 고정
    Vector3 newPos = origin + new Vector3(target.x * tileSize, target.y * tileSize, 0f);
    newPos.z = -2f;   // ✅ 항상 -2로 고정
    enemyStone.position = newPos;

    Debug.Log($"🔶 적 이동: {grid} → {target} (pos={newPos})");
    EndEnemyTurn();
}

    void EnemyAttack()
    {
    if (attackController == null) { EndEnemyTurn(); return; }

    string enemyName = string.IsNullOrEmpty(BattleContext.EnemyName) ? "Enemy1" : BattleContext.EnemyName;
    var t = FindTypeByName(enemyName);
    if (t == null) { Debug.LogWarning($"Enemy type not found: {enemyName}"); EndEnemyTurn(); return; }

    var go = new GameObject($"_EnemyPattern_{enemyName}");
    try
    {
        var comp = go.AddComponent(t) as ICardPattern;
        if (comp == null) { EndEnemyTurn(); return; }

        // 왼쪽(플레이어) 패널에 표시
        var timings = comp.Timings ?? new float[16];
        attackController.ShowPattern(comp.Pattern16, timings, AttackController.Panel.Player);

        float total = attackController.GetSequenceDuration(timings);
        Invoke(nameof(EndEnemyTurn), total);
    }
    finally
    {
        Destroy(go);
    }
    }

    void EndEnemyTurn()
    {
        Debug.Log("🔶 적 턴 종료");
        StartPlayerTurn();
    }

    static Type FindTypeByName(string typeName)
    {
        var asm = typeof(TurnManager).Assembly;
        return asm.GetTypes().FirstOrDefault(t => t.Name == typeName && typeof(MonoBehaviour).IsAssignableFrom(t));
    }
}
