// BattleDeckRuntime.cs (교체본)
using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleDeckRuntime : MonoBehaviour
{
    public static BattleDeckRuntime Instance { get; private set; }

    // 배틀용 덱(섞인 상태)
    public readonly List<string> deck = new();
    // 현재 손패
    public readonly List<string> hand = new();

    [Header("Rules")]
    [SerializeField] private int initialHandSize = 3; // 초기 드로우
    [SerializeField] private int maxHandSize = 3;     // 손패 최대

    // 손패가 바뀌면 UI가 구독해서 리빌드
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
        DrawInitial();                // 초기 드로우 끝에 OnHandChanged가 반드시 쏴짐
        if (hand.Count == 0)          // 혹시 초기 드로우가 0장일 수도 있으니 방어적으로 한 번 더
            OnHandChanged?.Invoke();
    }

    /// <summary>CardStateRuntime의 저장 덱을 읽어와 deck에 적재.</summary>
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

    /// <summary>초기 드로우.</summary>
    public void DrawInitial()
    {
        Draw(Mathf.Min(initialHandSize, maxHandSize)); // Draw 내부에서 OnHandChanged 호출
#if UNITY_EDITOR
        Debug.Log($"[BattleDeckRuntime] 초기 드로우 → [{string.Join(", ", hand)}]");
#endif
    }

    /// <summary>손패가 가득이 아니라면 1장 드로우.</summary>
    public void DrawOneIfNeeded()
    {
        if (hand.Count < maxHandSize) Draw(1);
    }

    /// <summary>n장 드로우. 실제로 이동이 있었다면 OnHandChanged 호출.</summary>
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

    /// <summary>손패에서 사용 → 덱 맨 밑으로.</summary>
    public bool UseCardToBottom(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count) return false;
        string id = hand[handIndex];
        hand.RemoveAt(handIndex);
        deck.Add(id);              // 맨 밑으로
        OnHandChanged?.Invoke();   // UI 갱신 통지
        return true;
    }

    /// <summary>UI에서 읽기용 손패 목록(읽기전용 뷰) 반환.</summary>
    public IReadOnlyList<string> GetHandIds() => hand;

    /// <summary>손패를 외부에서 통째로 교체해야 할 때 사용(테스트/효과 등).</summary>
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
