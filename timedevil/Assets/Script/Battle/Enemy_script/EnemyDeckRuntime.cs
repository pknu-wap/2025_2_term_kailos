// Assets/Script/Battle/EnemyDeckRuntime.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeckRuntime : MonoBehaviour
{
    public static EnemyDeckRuntime Instance { get; private set; }

    [Header("DB (EnemySO를 찾기 위해 필요)")]
    [SerializeField] private EnemyDatabaseSO enemyDatabase;

    public readonly List<string> deck = new();
    public readonly List<string> hand = new();

    [Header("Rules")]
    [SerializeField] private int initialHandSize = 3;
    [SerializeField] private int maxHandSize = 5;

    public event Action OnHandChanged;
    public int MaxHandSize => maxHandSize;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // 데이터 준비 상태가 씬마다 달라서, 플레이어쪽과 비슷하게 재시도 루틴 포함
        bool ok = TryInitOnce();
        if (!ok) StartCoroutine(CoRetryInit());
    }

    bool TryInitOnce()
    {
        deck.Clear();
        hand.Clear();

        if (!enemyDatabase) enemyDatabase = FindObjectOfType<EnemyDatabaseSO>(true);

        var enemyRt = EnemyRuntime.Instance ?? FindObjectOfType<EnemyRuntime>(true);
        if (!enemyRt || string.IsNullOrEmpty(enemyRt.enemyId)) return false;

        var so = enemyDatabase ? enemyDatabase.GetById(enemyRt.enemyId) : null;
        if (!so || so.deckIds == null || so.deckIds.Length == 0) return false;

        foreach (var id in so.deckIds)
            if (!string.IsNullOrEmpty(id)) deck.Add(id);

#if UNITY_EDITOR
        Debug.Log($"[EnemyDeckRuntime] 덱 로드 완료: {deck.Count}장 (EnemyId={enemyRt.enemyId})");
#endif
        Shuffle(deck);
        DrawInitial();
        if (hand.Count == 0) OnHandChanged?.Invoke();
        return true;
    }

    System.Collections.IEnumerator CoRetryInit()
    {
        for (int i = 0; i < 8; i++)
        {
            yield return null;
            if (TryInitOnce()) yield break;
        }
        OnHandChanged?.Invoke();
        Debug.LogWarning("[EnemyDeckRuntime] 초기화 재시도 실패(적 덱 비어 있음 또는 DB 참조 누락).");
    }

    public static void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public void DrawInitial()
    {
        Draw(Mathf.Min(initialHandSize, maxHandSize));
#if UNITY_EDITOR
        Debug.Log($"[EnemyDeckRuntime] 초기 드로우 → [{string.Join(", ", hand)}]");
#endif
    }

    public void DrawOneIfNeeded()
    {
        if (hand.Count < maxHandSize) Draw(1);
    }

    public void Draw(int n)
    {
        bool moved = false;
        for (int k = 0; k < n; k++)
        {
            if (hand.Count >= maxHandSize) break;
            if (deck.Count == 0) break;

            string top = deck[0];
            deck.RemoveAt(0);
            hand.Add(top);
            moved = true;
        }
        if (moved) OnHandChanged?.Invoke();
    }

    public bool UseCardToBottom(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count) return false;
        string id = hand[handIndex];
        hand.RemoveAt(handIndex);
        deck.Add(id);
        OnHandChanged?.Invoke();
        return true;
    }

    public IReadOnlyList<string> GetHandIds() => hand;

    public void SetHand(List<string> newHand)
    {
        hand.Clear();
        if (newHand != null)
        {
            foreach (var id in newHand)
                if (!string.IsNullOrEmpty(id)) hand.Add(id);
        }
        OnHandChanged?.Invoke();
    }
}
