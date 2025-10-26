// PlayerDataRuntime.cs
using UnityEngine;

public class PlayerDataRuntime : MonoBehaviour
{
    public static PlayerDataRuntime Instance { get; private set; }

    [Header("Auto Save 옵션")]
    public bool saveOnDisable = false;
    public bool saveOnQuit = true;

    [Header("Data")]
    public PlayerData Data;   // 인스펙터에서 기본값 설정 가능

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // 씬에 PlayerDataRuntime가 이미 있고 Data가 비어있다면 파일에서 로드
        if (Data == null)
            Data = PlayerDataStore.Load();

        // 파일에도 없으면 기본값 생성 (원하면 이 부분 삭제 가능)
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
