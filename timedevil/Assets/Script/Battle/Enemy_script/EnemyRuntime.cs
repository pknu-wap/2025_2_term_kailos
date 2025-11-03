using UnityEngine;

public class EnemyRuntime : MonoBehaviour
{
    public static EnemyRuntime Instance { get; private set; }

    // Identity
    public string EnemyId { get; private set; }
    public string DisplayName { get; private set; }

    // Stats
    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }
    public int ATK { get; private set; }
    public int DEF { get; private set; }
    public int SPD { get; private set; }   // 표준 이름

    // TurnManager 호환용(alias): enemyRt.speed 로 읽어도 OK
    public int speed => SPD;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void InitializeFromSO(EnemySO so)
    {
        if (!so) { Debug.LogError("[EnemyRuntime] SO is null"); return; }

        EnemyId = string.IsNullOrEmpty(so.enemyId) ? "Enemy" : so.enemyId;
        DisplayName = string.IsNullOrEmpty(so.displayName) ? EnemyId : so.displayName;

        MaxHP = Mathf.Max(1, so.baseHP);
        CurrentHP = MaxHP;
        ATK = Mathf.Max(0, so.baseATK);
        DEF = Mathf.Max(0, so.baseDEF);
        SPD = Mathf.Max(0, so.baseSPD);
    }

    // (레거시 Enemy1 모노비헤이비어 폴백용 – 필요 없으면 지워도 됨)
    public void InitializeFromFallback(MonoBehaviour legacyComp)
    {
        if (!legacyComp) return;
        var t = legacyComp.GetType();

        int ReadInt(string name, int def)
        {
            var f = t.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (f == null) return def;
            try { return Mathf.Max(0, (int)f.GetValue(legacyComp)); } catch { return def; }
        }

        EnemyId = t.Name;
        DisplayName = t.Name;

        MaxHP = ReadInt("maxHP", 1);
        CurrentHP = Mathf.Clamp(ReadInt("currentHP", MaxHP), 0, MaxHP);
        ATK = ReadInt("attack", 0);
        DEF = ReadInt("defense", 0);
        SPD = ReadInt("speed", 0);
    }

    public void ApplyDamage(int dmg)
    {
        if (dmg <= 0) return;
        CurrentHP = Mathf.Max(0, CurrentHP - dmg);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
    }
}
