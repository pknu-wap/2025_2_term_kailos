// PlayerDataRuntime.cs
using UnityEngine;

public class PlayerDataRuntime : MonoBehaviour
{
    public static PlayerDataRuntime Instance { get; private set; }

    [Header("Auto Save �ɼ�")]
    public bool saveOnDisable = false;
    public bool saveOnQuit = true;

    [Header("Data")]
    public PlayerData Data;   // �ν����Ϳ��� �⺻�� ���� ����

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        DontDestroyOnLoad(gameObject);   // �� �� ��ȯ ����

        if (Data == null)
            Data = PlayerDataStore.Load();
        if (Data == null)
        {
            Data = new PlayerData();
            Data.InitDefaults("Player", 100, 10, 5, 5);
        }
    }

    public void SaveNow()
    {
        PlayerDataStore.Save(Data);
    }

    void OnDisable()
    {
        if (saveOnDisable) SaveNow();
    }

    void OnApplicationQuit()
    {
        if (saveOnQuit) SaveNow();
    }
}
