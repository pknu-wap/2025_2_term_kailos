using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeckRuntime : MonoBehaviour, IHandReadable
{
    public static EnemyDeckRuntime Instance { get; private set; }

    // 전투용 덱 / 손패
    public readonly List<string> deck = new();
    public readonly List<string> hand = new();

    [Header("Rules")]
    [SerializeField] private int maxHandSize = 3;

    public event Action OnHandChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>외부(EnemySO 등)에서 적 덱을 주입</summary>
    public void LoadDeckFromList(List<string> source)
    {
        deck.Clear();
        hand.Clear();
        if (source != null)
            foreach (var id in source)
                if (!string.IsNullOrEmpty(id)) deck.Add(id);
        OnHandChanged?.Invoke();
    }

    public void Shuffle()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
    }

    /// <summary>손패가 가득이 아니라면 딱 1장만 드로우</summary>
    public void DrawOneIfNeeded()
    {
        if (hand.Count >= maxHandSize) return;
        if (deck.Count == 0) return;
        string top = deck[0];
        deck.RemoveAt(0);
        hand.Add(top);
        OnHandChanged?.Invoke();
    }

    /// <summary>손패 인덱스를 덱 맨 아래로 이동</summary>
    public bool UseCardToBottom(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count) return false;
        string id = hand[handIndex];
        hand.RemoveAt(handIndex);
        deck.Add(id);
        OnHandChanged?.Invoke();
        return true;
    }

    // IHandReadable
    public IReadOnlyList<string> GetHandIds() => hand;

    /// <summary>디버그용: 손패를 교체</summary>
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
