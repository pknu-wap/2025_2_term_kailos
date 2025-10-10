// PlayerDataRuntime.cs
using UnityEngine;

public class PlayerDataRuntime : MonoBehaviour
{
    public static PlayerDataRuntime Instance { get; private set; }

    [Header("Active Player Data (Runtime)")]
    public PlayerData data = new PlayerData();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 첫 실행 시 기본값 보장
        if (data == null) data = new PlayerData();
        if (string.IsNullOrEmpty(data.playerName)) data.InitDefaults();
    }

    // 편의 메서드들 (선택)
    public void SetDefaults(string name = "Player", int hp = 100, int atk = 10, int def = 5, int spd = 5)
        => data.InitDefaults(name, hp, atk, def, spd);

    public PlayerData Snapshot()
        => JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(data)); // 얕은 스냅샷
}
