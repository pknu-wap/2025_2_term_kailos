using UnityEngine;
using System.Linq;
using System.IO;

public class CardStateRuntime : MonoBehaviour
{
    public static CardStateRuntime Instance { get; private set; }

    [Header("자동 저장 옵션 (기본 꺼짐)")]
    public bool saveOnDisable = false;
    public bool saveOnQuit = false;

    public CardSaveData Data { get; private set; }

    void Awake()
    {
        // ✅ 싱글톤 + 씬 전환 유지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 파일이 없으면 빈 상태로 시작(자동 생성 X)
        Data = CardSaveStore.Load();

#if UNITY_EDITOR
        Debug.Log($"[CardStateRuntime] Loaded. owned={Data.owned?.Count ?? 0}, deck={Data.deck?.Count ?? 0}");
#endif
    }

    void OnDisable()
    {
        if (!saveOnDisable) return;
        if (ShouldSkipEmptyInitialSave()) return;
        CardSaveStore.Save(Data);
    }

    void OnApplicationQuit()
    {
        if (!saveOnQuit) return;
        if (ShouldSkipEmptyInitialSave()) return;
        CardSaveStore.Save(Data);
    }

    public void SaveNow()
    {
        CardSaveStore.Save(Data);
#if UNITY_EDITOR
        Debug.Log("[CardStateRuntime] SaveNow → " + CardSaveStore.GetPath());
#endif
    }

    public bool AddOwned(string cardId)
    {
        if (Data.owned == null) Data.owned = new System.Collections.Generic.List<string>();
        if (!Data.owned.Contains(cardId))
        {
            Data.owned.Add(cardId);
            return true;
        }
        return false;
    }

    public bool RemoveOwned(string cardId)
    {
        if (Data.owned == null) return false;
        bool removed = Data.owned.Remove(cardId);
        if (removed && Data.deck != null)
            Data.deck = Data.deck.Where(id => id != cardId).ToList();
        return removed;
    }

    public void SetDeck(System.Collections.Generic.IEnumerable<string> ids)
    {
        Data.deck = ids?.ToList() ?? new System.Collections.Generic.List<string>();
    }

    // --- Helpers ---
    private bool ShouldSkipEmptyInitialSave()
    {
        return IsEmpty(Data) && !File.Exists(CardSaveStore.GetPath());
    }
    private static bool IsEmpty(CardSaveData d)
    {
        if (d == null) return true;
        int ownedCount = d.owned?.Count ?? 0;
        int deckCount = d.deck?.Count ?? 0;
        return ownedCount == 0 && deckCount == 0;
    }
}
