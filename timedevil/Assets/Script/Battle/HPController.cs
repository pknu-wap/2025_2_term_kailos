using UnityEngine;

public class HPController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerDataRuntime playerData;
    [SerializeField] private EnemyRuntime enemyData;

    [Header("Pawns (for hit test)")]
    [SerializeField] private Transform playerPawn;
    [SerializeField] private Transform enemyPawn;

    // 공격 중 타겟 팩션(판정 시 위치 참조용). AttackController가 설정/참조.
    public Faction CurrentDamageTarget { get; private set; } = Faction.Enemy;

    void Awake()
    {
        if (!playerData) playerData = FindObjectOfType<PlayerDataRuntime>(true);
        if (!enemyData) enemyData = EnemyRuntime.Instance ?? FindObjectOfType<EnemyRuntime>(true);
        // Pawn 레퍼런스는 MoveController 등에서 쓰던 걸 그대로 넣어주면 됨.
        if (!playerPawn)
        {
            var mc = FindObjectOfType<MoveController>(true);
            if (mc) playerPawn = mc.GetComponentInChildren<Transform>(); // 필요하면 직접 연결해줘
        }
        if (!enemyPawn)
        {
            var mc = FindObjectOfType<MoveController>(true);
            if (mc) enemyPawn = mc.GetComponentInChildren<Transform>(); // 필요하면 직접 연결해줘
        }
    }

    // ---------- 공개 API ----------

    public int GetAttack(Faction who)
    {
        if (who == Faction.Player) return ReadIntFieldOrProp(playerData?.Data, "atk", 0);
        else return ReadIntFieldOrProp(enemyData, "atk", 0);
    }

    public int GetDefense(Faction who)
    {
        if (who == Faction.Player) return ReadIntFieldOrProp(playerData?.Data, "def", 0);
        else return ReadIntFieldOrProp(enemyData, "def", 0);
    }

    public int GetHP(Faction who)
    {
        if (who == Faction.Player) return ReadIntFieldOrProp(playerData?.Data, "hp", 0);
        else return ReadIntFieldOrProp(enemyData, "hp", 0);
    }

    public void ApplyDamage(Faction target, int amount)
    {
        amount = Mathf.Max(0, amount);
        CurrentDamageTarget = target; // 이후 판정에서 참조 가능

        if (target == Faction.Player)
        {
            int hp = GetHP(Faction.Player);
            hp = Mathf.Max(0, hp - amount);
            WriteIntFieldOrProp(playerData?.Data, "hp", hp);
            Debug.Log($"[HP] Player -{amount} → {hp}");
        }
        else
        {
            int hp = GetHP(Faction.Enemy);
            hp = Mathf.Max(0, hp - amount);
            WriteIntFieldOrProp(enemyData, "hp", hp);
            Debug.Log($"[HP] Enemy -{amount} → {hp}");
        }

        // TODO: HP UI 반영(게이지), 사망 처리 등은 여기에서 Hook
    }

    public Vector3 GetWorldPositionOfPawn(Faction who)
    {
        if (who == Faction.Player && playerPawn) return playerPawn.position;
        if (who == Faction.Enemy && enemyPawn) return enemyPawn.position;
        return Vector3.positiveInfinity;
    }

    // ---------- 리플렉션 보조 ----------

    private int ReadIntFieldOrProp(object obj, string name, int fallback)
    {
        if (obj == null) return fallback;

        var t = obj.GetType();
        const System.Reflection.BindingFlags BF =
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic;

        var f = t.GetField(name, BF);
        if (f != null && f.FieldType == typeof(int)) return (int)f.GetValue(obj);

        var p = t.GetProperty(name, BF);
        if (p != null && p.PropertyType == typeof(int) && p.CanRead) return (int)p.GetValue(obj);

        return fallback;
    }

    private void WriteIntFieldOrProp(object obj, string name, int value)
    {
        if (obj == null) return;

        var t = obj.GetType();
        const System.Reflection.BindingFlags BF =
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic;

        var f = t.GetField(name, BF);
        if (f != null && f.FieldType == typeof(int)) { f.SetValue(obj, value); return; }

        var p = t.GetProperty(name, BF);
        if (p != null && p.PropertyType == typeof(int) && p.CanWrite) { p.SetValue(obj, value); return; }

        Debug.LogWarning($"[HPController] '{t.Name}'에 '{name}'(int) 쓰기 실패.");
    }

    public void BeginCardHitTest(Faction target)
    {
        CurrentDamageTarget = target;
    }
}
