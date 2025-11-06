using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleDeckRuntime : MonoBehaviour
{
    public static BattleDeckRuntime Instance { get; private set; }

    public readonly List<string> deck = new();
    public readonly List<string> hand = new();

    [Header("Rules")]
    [SerializeField] private int initialHandSize = 5;
    [SerializeField] private int maxHandSize = 5;

    public event Action OnHandChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // 1) 즉시 시도
        bool okNow = TryInitOnce();
        if (!okNow)
        {
            // 2) 준비가 안 돼 있으면 몇 프레임 기다렸다가 한 번 더
            StartCoroutine(CoRetryInit());
        }
    }

    bool TryInitOnce()
    {
        deck.Clear();
        hand.Clear();

        var rt = CardStateRuntime.Instance;
        var src = rt != null ? rt.Data?.deck : null;

        if (src == null || src.Count == 0)
        {
            // 아직 저장이 안 올라왔거나 덱이 비어있음
            return false;
        }

        foreach (var id in src)
            if (!string.IsNullOrEmpty(id)) deck.Add(id);

#if UNITY_EDITOR
        Debug.Log($"[BattleDeckRuntime] 덱 로드 완료: {deck.Count}장");
#endif
        Shuffle(deck);
        DrawInitial();                    // 내부에서 OnHandChanged 발생 가능
        if (hand.Count == 0) OnHandChanged?.Invoke(); // 그래도 한 번 보장
        return true;
    }

    System.Collections.IEnumerator CoRetryInit()
    {
        // 최대 8프레임 정도만 기다렸다가 한 번 더 시도
        for (int i = 0; i < 8; i++)
        {
            yield return null;
            if (TryInitOnce()) yield break;
        }

        // 그래도 안 되면 최소 한 번은 UI를 갱신시켜 빈 상태를 반영
        OnHandChanged?.Invoke();
        Debug.LogWarning("[BattleDeckRuntime] 초기화 재시도 실패(덱 비어 있음). 저장/씬 세팅 확인 필요");
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
