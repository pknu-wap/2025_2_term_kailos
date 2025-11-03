// Assets/Script/Battle/BattleDeckRuntime.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleDeckRuntime : MonoBehaviour, IHandReadable   // ⬅︎ 인터페이스 추가
{
    public static BattleDeckRuntime Instance { get; private set; }

    // 배틀용 덱(섞인 상태)
    public readonly List<string> deck = new();
    // 현재 손패
    public readonly List<string> hand = new();

    [Header("Rules")]
    [SerializeField] private int initialHandSize = 3; // 필요에 맞게
    [SerializeField] private int maxHandSize = 3;

    // HandUI가 구독하는 이벤트
    public event Action OnHandChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        LoadDeckFromRuntime();
        Shuffle(deck);
        DrawInitial();
        if (hand.Count == 0) OnHandChanged?.Invoke();
    }

    public void LoadDeckFromRuntime()
    {
        deck.Clear();
        hand.Clear();

        var rt = CardStateRuntime.Instance;
        var src = rt != null ? rt.Data?.deck : null;

        if (src == null || src.Count == 0)
        {
            Debug.LogWarning("[BattleDeckRuntime] 덱이 비어 있음");
            return;
        }

        foreach (var id in src)
            if (!string.IsNullOrEmpty(id)) deck.Add(id);

        Debug.Log($"[BattleDeckRuntime] 덱 로드 완료: {deck.Count}장");
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
        Debug.Log($"[BattleDeckRuntime] 초기 드로우 → [{string.Join(", ", hand)}]");
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

    // IHandReadable 구현
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
