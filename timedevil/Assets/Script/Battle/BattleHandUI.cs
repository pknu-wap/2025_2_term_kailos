using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleHandUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform cardGroup;   // 카드가 배치될 부모(= card_group)
    [SerializeField] private GameObject cardUiPrefab;   // CardUI 프리팹 (Image+Button+CardUI)
    [SerializeField] private string resourcesFolder = "my_asset";
    [SerializeField] private AttackController attackController;

    [Header("Layout")]
    [SerializeField] private float cardWidth = 120f;
    [SerializeField] private float maxSpacing = 140f;   // 손패가 적을 때 간격
    [SerializeField] private float minSpacing = 40f;    // 손패가 많을 때 겹치기 간격
    [SerializeField] private float horizontalPadding = 40f;

    // 내부
    readonly List<CardUI> spawned = new();
    bool usedThisTurn = false;   // 한 턴 1회 사용 제한

    void Start()
    {
        // 첫 화면: BattleDeckRuntime 가 Start()에서 초기 드로우를 끝냄
        Refresh();
    }

    // TurnManager에서 플레이어 턴 시작 시 호출해 주세요
    public void OnPlayerTurnStart()
    {
        usedThisTurn = false;
        BattleDeckRuntime.Instance?.DrawOneIfNeeded();
        Refresh();
    }

    // 손패 표시 다시 만들기
    public void Refresh()
    {
        if (cardGroup == null || cardUiPrefab == null) return;

        // 기존 제거
        foreach (var c in spawned) if (c) Destroy(c.gameObject);
        spawned.Clear();

        var deck = BattleDeckRuntime.Instance;
        if (deck == null || deck.hand.Count == 0) return;

        int n = deck.hand.Count;

        // 가변 간격 계산
        float width = cardGroup.rect.width - horizontalPadding * 2f;
        float spacing = Mathf.Clamp(width / Mathf.Max(1, n - 1), minSpacing, maxSpacing);

        // 좌측 기준 x 시작점
        float startX = -0.5f * (spacing * (n - 1));

        for (int i = 0; i < n; i++)
        {
            string id = deck.hand[i];

            var go = Instantiate(cardUiPrefab, cardGroup);
            var ui = go.GetComponent<CardUI>();
            if (ui == null) ui = go.AddComponent<CardUI>();

            var sp = Resources.Load<Sprite>($"{resourcesFolder}/{id}");
            ui.Init(this, i, sp);

            // 위치 (수평 배열)
            var rt = go.transform as RectTransform;
            rt.anchoredPosition = new Vector2(startX + spacing * i, 0f);
            rt.sizeDelta = new Vector2(cardWidth, rt.sizeDelta.y);

            spawned.Add(ui);
        }
    }

    // 카드 클릭 콜백 (CardUI → 여기로)
    public void OnClickCard(int handIndex)
    {
        // 턴/제한 체크
        if (TurnManager.Instance == null || TurnManager.Instance.currentTurn != TurnState.PlayerTurn)
        {
            Debug.Log("[BattleHandUI] 지금은 플레이어 턴이 아님");
            return;
        }
        if (usedThisTurn)
        {
            Debug.Log("[BattleHandUI] 이번 턴에는 이미 카드를 사용함");
            return;
        }

        var deck = BattleDeckRuntime.Instance;
        if (deck == null) return;
        if (handIndex < 0 || handIndex >= deck.hand.Count) return;

        string id = deck.hand[handIndex];

        // CardX.cs 타입을 찾아서 패턴 꺼내기
        var t = FindTypeByName(id);
        if (t == null)
        {
            Debug.LogWarning($"[BattleHandUI] 카드 타입을 못 찾음: {id}");
            return;
        }

        StartCoroutine(Co_UseCardAndAttack(handIndex, t));
    }

    IEnumerator Co_UseCardAndAttack(int handIndex, Type cardType)
    {
        usedThisTurn = true;

        // 임시 오브젝트로 ICardPattern 읽기
        var go = new GameObject($"_PlayerCard_{cardType.Name}");
        float total = 0f;

        try
        {
            var comp = go.AddComponent(cardType) as ICardPattern;
            if (comp == null) yield break;

            var timings = comp.Timings ?? new float[16];

            // 적 보드에 공격 연출
            attackController.ShowPattern(comp.Pattern16, timings, AttackController.Panel.Enemy);
            total = attackController.GetSequenceDuration(timings);
        }
        finally
        {
            Destroy(go);
        }

        // 카드 사용 → 덱 맨밑으로
        BattleDeckRuntime.Instance.UseCardToBottom(handIndex);
        Refresh(); // 손패 즉시 반영

        // 연출 끝날 때까지 기다리고 턴 종료
        if (total > 0f) yield return new WaitForSeconds(total);
        TurnManager.Instance.EndPlayerTurn();
    }

    static Type FindTypeByName(string typeName)
    {
        var asm = typeof(BattleHandUI).Assembly;
        return asm.GetTypes().FirstOrDefault(t => t.Name == typeName && typeof(MonoBehaviour).IsAssignableFrom(t));
    }
}
