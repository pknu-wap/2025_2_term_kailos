using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 배틀 씬 전용 덱/핸드 런타임:
/// - CardStateRuntime의 deck을 복사해 섞고
/// - 배틀 시작 시 3장 랜덤 드로우
/// - (이후 확장) 한 턴에 1장 사용 → 맨 밑으로 이동, 손패 3장 미만이면 드로우 등
/// </summary>
public class BattleDeckRuntime : MonoBehaviour
{
    public static BattleDeckRuntime Instance { get; private set; }

    /// <summary>배틀용 덱(섞인 상태). CardId 나열.</summary>
    public readonly List<string> deck = new();

    /// <summary>현재 손패(최대 3장).</summary>
    public readonly List<string> hand = new();

    [Header("Rules")]
    [Tooltip("초기 드로우 장수")]
    [SerializeField] private int initialHandSize = 3;
    [Tooltip("손패 최대 장수")]
    [SerializeField] private int maxHandSize = 3;

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
    }

    /// <summary>CardStateRuntime의 deck을 복사.</summary>
    public void LoadDeckFromRuntime()
    {
        deck.Clear();
        hand.Clear();

        var rt = CardStateRuntime.Instance;
        var src = rt != null ? rt.Data?.deck : null;

        if (src == null || src.Count == 0)
        {
            Debug.LogWarning("[BattleDeckRuntime] 덱이 비어 있음. 초기 드로우 불가");
            return;
        }

        // 중복 금지 정책 유지: 이미 덱이 중복 없이 만들어져 있다면 그대로 복사
        foreach (var id in src)
            if (!string.IsNullOrEmpty(id)) deck.Add(id);

        Debug.Log($"[BattleDeckRuntime] 덱 로드 완료: {deck.Count}장");
    }

    /// <summary>Fisher–Yates 셔플.</summary>
    public static void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>배틀 시작 시 초기 드로우.</summary>
    public void DrawInitial()
    {
        Draw(Mathf.Min(initialHandSize, maxHandSize));
#if UNITY_EDITOR
        Debug.Log($"[BattleDeckRuntime] 초기 드로우 → [{string.Join(", ", hand)}]");
#endif
    }

    /// <summary>n장 드로우(덱이 모자라면 가능한 만큼만).</summary>
    public void Draw(int n)
    {
        for (int k = 0; k < n; k++)
        {
            if (hand.Count >= maxHandSize) break;
            if (deck.Count == 0) break;

            // 맨 위(리스트 뒤에서 꺼내도, 앞에서 꺼내도 상관 없음)에서 1장
            string top = deck[0];
            deck.RemoveAt(0);
            hand.Add(top);
        }
        // 손패 갱신 이벤트 등을 나중에 붙일 수 있음
    }

    /// <summary>손패에서 특정 인덱스 카드를 사용 → 그 카드를 덱 맨 밑으로.</summary>
    public bool UseCardToBottom(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count) return false;
        string id = hand[handIndex];
        hand.RemoveAt(handIndex);
        deck.Add(id); // 맨 밑으로
        return true;
    }

    /// <summary>손패가 3장 미만이면 1장 드로우(플레이어 턴 시작 시 호출 예정).</summary>
    public void DrawOneIfNeeded()
    {
        if (hand.Count < maxHandSize) Draw(1);
    }
}
