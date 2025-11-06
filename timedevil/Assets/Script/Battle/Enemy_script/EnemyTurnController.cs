// Assets/Script/Battle/Enemy_script/EnemyTurnController.cs
using System.Collections;
using System.Reflection;
using UnityEngine;

public class EnemyTurnController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EnemyDeckRuntime enemyDeck;
    [SerializeField] private CardDatabaseSO cardDatabase;
    [SerializeField] private CostController cost;
    [SerializeField] private ShowCardController showCard;

    [Header("Timings")]
    [SerializeField] private float previewSeconds = 1.2f;
    [SerializeField] private float playInterval = 0.15f;

    void Awake()
    {
        if (!enemyDeck) enemyDeck = EnemyDeckRuntime.Instance ?? FindObjectOfType<EnemyDeckRuntime>(true);
        if (!cardDatabase)
            cardDatabase = Resources.Load<CardDatabaseSO>("CardDatabase");
        if (!cost) cost = FindObjectOfType<CostController>(true);
        if (!showCard) showCard = FindObjectOfType<ShowCardController>(true);


        Debug.Log($"[EnemyTurn] Controller bound on: {gameObject.scene.name}/{gameObject.name}");

    }

    public IEnumerator RunTurn()
    {
        if (enemyDeck == null || cost == null) yield break;

        // 손패 모자라면 1장 드로우
        if (enemyDeck.GetHandIds().Count < enemyDeck.MaxHandSize)
            enemyDeck.DrawOneIfNeeded();

        while (true)
        {
            var hand = enemyDeck.GetHandIds();
            if (hand == null || hand.Count == 0)
            {
                Debug.Log("[EnemyTurn] 손패가 비어 턴 종료");
                yield break;
            }

            // 진단 로그: 현재 손패 + 남은 코스트
            Debug.Log($"[EnemyTurn] Hand= [{string.Join(", ", hand)}], Cost={cost.Current}");

            int playableIndex = -1;
            int playableCost = int.MaxValue;
            string playableId = null;

            for (int i = 0; i < hand.Count; i++)
            {
                string id = hand[i];
                int c = GetCardCost(id);
                Debug.Log($"[EnemyTurn] probe id={id}, cost={c}");
                if (c <= cost.Current)
                {
                    playableIndex = i;
                    playableCost = c;
                    playableId = id;
                    break;
                }
            }

            if (playableIndex < 0)
            {
                Debug.Log("[EnemyTurn] 낼 수 있는 카드가 없어 턴 종료");
                yield break;
            }

            if (!cost.TryPay(playableCost))
            {
                Debug.LogWarning("[EnemyTurn] 코스트 지불 실패 → 턴 종료");
                yield break;
            }

            Debug.Log($"[EnemyTurn] Play '{playableId}' (cost={playableCost})");

            if (showCard != null)
                yield return showCard.PreviewById(playableId, previewSeconds);

            enemyDeck.UseCardToBottom(playableIndex);

            if (playInterval > 0f)
                yield return new WaitForSeconds(playInterval);
        }
    }

    // ---- 코스트 추출을 견고하게 (필드명이 달라도 최대한 잡아줌) ----
    private int GetCardCost(string id)
    {
        if (string.IsNullOrEmpty(id) || cardDatabase == null)
        {
            Debug.LogWarning("[EnemyTurn] cost fail: empty id or CardDatabase missing");
            return 9999;
        }

        var so = cardDatabase.GetById(id);
        if (!so)
        {
            Debug.LogWarning($"[EnemyTurn] cost fail: DB miss for id='{id}'");
            return 9999;
        }

        // ✅ 자식 타입 먼저, 부모는 마지막
        if (so is AttackCardSO a) return Mathf.Max(0, a.cost);
        if (so is MoveCardSO m) return Mathf.Max(0, m.cost);
        if (so is SupportCardSO s) return Mathf.Max(0, s.cost);
        if (so is BaseCardSO b) return Mathf.Max(0, b.cost);

        // ✅ 폴백: public/private field/property 모두 탐색
        var t = so.GetType();
        const System.Reflection.BindingFlags BF =
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic;

        // field: "cost"/"Cost"
        var f = t.GetField("cost", BF) ?? t.GetField("Cost", BF);
        if (f != null && f.FieldType == typeof(int))
            return Mathf.Max(0, (int)f.GetValue(so));

        // property: "cost"/"Cost"
        var p = t.GetProperty("cost", BF) ?? t.GetProperty("Cost", BF);
        if (p != null && p.PropertyType == typeof(int) && p.CanRead)
            return Mathf.Max(0, (int)p.GetValue(so));

        Debug.LogWarning($"[EnemyTurn] cost fail: type '{t.Name}' has no int cost for id='{id}'");
        return 9999;
    }
}
