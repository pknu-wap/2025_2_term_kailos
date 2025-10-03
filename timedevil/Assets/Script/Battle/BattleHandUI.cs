using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleHandUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform cardGroup;      // 카드가 놓일 부모(항상 Active)
    [SerializeField] private GameObject cardUiPrefab;      // Image+Button+CardUI 달린 프리팹
    [SerializeField] private string resourcesFolder = "my_asset";
    [SerializeField] private AttackController attackController;

    [Header("Layout")]
    [SerializeField] private float cardWidth = 120f;
    [SerializeField] private float maxSpacing = 140f;      // 손패 적을 때 간격
    [SerializeField] private float minSpacing = 40f;       // 손패 많을 때(겹치기) 간격
    [SerializeField] private float horizontalPadding = 40f;

    private readonly List<CardUI> spawned = new();
    private bool usedThisTurn = false;
    private CanvasGroup cg;                                // card_group의 CanvasGroup

    void Awake()
    {
        if (!cardGroup)
        {
            Debug.LogError("[BattleHandUI] cardGroup 미지정");
            return;
        }
        cg = cardGroup.GetComponent<CanvasGroup>();
        if (!cg) cg = cardGroup.gameObject.AddComponent<CanvasGroup>();
        SetVisible(false); // 시작은 닫힘
    }

    void Start()
    {
        Refresh(); // 데이터는 그려두되, 표시는 TurnManager가 제어
    }

    // ---------- 표시/숨김 ----------
    public void SetVisible(bool on)
    {
        if (!cg) return;
        cg.alpha = on ? 1f : 0f;
        cg.interactable = on;
        cg.blocksRaycasts = on;
        Debug.Log($"[BattleHandUI] SetVisible({on}) → alpha={cg.alpha}, interactable={cg.interactable}, blocks={cg.blocksRaycasts}");

    }
    public bool IsVisible() => cg && cg.alpha > 0.5f && cg.blocksRaycasts;
    public void OpenAndRefresh() { SetVisible(true); Refresh(); }
    public void Close() => SetVisible(false);

    // ---------- 턴 진입 ----------
    public void OnPlayerTurnStart()
    {
        usedThisTurn = false;
        BattleDeckRuntime.Instance?.DrawOneIfNeeded(); // 3장 미만이면 1장 드로우
        Refresh();
        SetVisible(false); // 기본은 닫아둠(카드 버튼으로 열기)
    }

    // ---------- 렌더링 ----------
    public void Refresh()
    {
        if (!cardGroup || !cardUiPrefab) return;

        foreach (var c in spawned) if (c) Destroy(c.gameObject);
        spawned.Clear();

        var deck = BattleDeckRuntime.Instance;
        if (deck == null || deck.hand.Count == 0) return;

        int n = deck.hand.Count;
        float width = cardGroup.rect.width - horizontalPadding * 2f;
        float spacing = Mathf.Clamp(width / Mathf.Max(1, n - 1), minSpacing, maxSpacing);
        float startX = -0.5f * (spacing * (n - 1));

        for (int i = 0; i < n; i++)
        {
            string id = deck.hand[i];

            var go = Instantiate(cardUiPrefab, cardGroup);
            var ui = go.GetComponent<CardUI>() ?? go.AddComponent<CardUI>();

            // 예: Resources/my_asset/Card5.png
            var sp = Resources.Load<Sprite>($"{resourcesFolder}/{id}");
            ui.Init(this, i, sp);

            // 수평 배치
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = new Vector2(startX + spacing * i, 0f);
            rt.sizeDelta = new Vector2(cardWidth, rt.sizeDelta.y);

            spawned.Add(ui);
        }
    }

    // ---------- 클릭 -> 사용/공격/턴종료 ----------
    public void OnClickCard(int handIndex)
    {
        if (!IsVisible()) return;
        if (TurnManager.Instance == null ||
            TurnManager.Instance.currentTurn != TurnState.PlayerTurn) return;
        if (usedThisTurn) return;

        var deck = BattleDeckRuntime.Instance;
        if (deck == null || handIndex < 0 || handIndex >= deck.hand.Count) return;

        string id = deck.hand[handIndex];
        var t = FindTypeByName(id);
        if (t == null)
        {
            Debug.LogWarning($"[BattleHandUI] 카드 타입 없음: {id}");
            return;
        }

        StartCoroutine(Co_UseCardAndAttack(handIndex, t));
    }

    private IEnumerator Co_UseCardAndAttack(int handIndex, Type cardType)
    {
        usedThisTurn = true;
        SetVisible(false); // 입력 차단 & 숨김

        // 패턴 정보 획득용 임시 GO
        var tmp = new GameObject($"_PlayerCard_{cardType.Name}");
        float total = 0f;
        try
        {
            var comp = tmp.AddComponent(cardType) as ICardPattern;
            if (comp == null) yield break;

            var timings = comp.Timings ?? new float[16];
            attackController.ShowPattern(comp.Pattern16, timings, AttackController.Panel.Enemy);
            total = attackController.GetSequenceDuration(timings);
        }
        finally
        {
            Destroy(tmp);
        }

        // 사용 카드 → 덱 맨 아래
        BattleDeckRuntime.Instance.UseCardToBottom(handIndex);
        Refresh(); // 손패 즉시 반영(닫힌 상태에서도 내부 데이터 갱신)

        if (total > 0f) yield return new WaitForSeconds(total);
        TurnManager.Instance.EndPlayerTurn();
    }

    // ---------- 유틸 ----------
    private static Type FindTypeByName(string typeName)
    {
        var asm = typeof(BattleHandUI).Assembly;
        return asm.GetTypes().FirstOrDefault(
            t => t.Name == typeName && typeof(MonoBehaviour).IsAssignableFrom(t));
    }
}
