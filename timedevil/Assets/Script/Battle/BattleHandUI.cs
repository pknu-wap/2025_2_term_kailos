using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// - 자기 자신(RectTransform)을 손패 컨테이너로 사용
/// - CanvasGroup으로 표시/입력 제어
/// - 카드 클릭 시 ICardPattern 실행 → AttackController 연출 → 턴 종료
/// </summary>
public class BattleHandUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject cardUiPrefab;      // Image+Button+CardUI 프리팹
    [SerializeField] private string resourcesFolder = "my_asset";
    [SerializeField] private AttackController attackController;

    [Header("Layout")]
    [SerializeField] private float cardWidth = 120f;
    [SerializeField] private float maxSpacing = 140f;      // 손패 적을 때
    [SerializeField] private float minSpacing = 40f;       // 손패 많을 때(겹침)
    [SerializeField] private float horizontalPadding = 40f;

    // 내부 상태
    private readonly List<CardUI> spawned = new();
    private bool usedThisTurn = false;
    private CanvasGroup cg;                 // 자기 자신에 붙은 CanvasGroup
    private RectTransform group;            // 자기 자신 RectTransform

    void Awake()
    {
        group = (RectTransform)transform;
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();

        SetVisible(false); // 시작은 닫힘
        Debug.Log("[BattleHandUI] Awake() 초기화 완료");
    }

    // --------------- 공개 API (TurnManager에서 사용) ---------------

    /// <summary>플레이어 턴 시작 시 TurnManager가 호출.</summary>
    public void OnPlayerTurnStart()
    {
        usedThisTurn = false;
        BattleDeckRuntime.Instance?.DrawOneIfNeeded(); // 3장 미만이면 1장 드로우
        Refresh();
        SetVisible(false, "OnPlayerTurnStart");        // 기본은 닫힌 상태(카드 버튼으로 열기)
    }

    /// <summary>TurnManager: 카드 버튼 눌렸을 때 호출.</summary>
    public void OpenAndRefresh()
    {
        SetVisible(true, "OpenAndRefresh");
        Refresh();
    }

    /// <summary>TurnManager: 강제로 닫아야 할 때 호출.</summary>
    public void Close()
    {
        SetVisible(false);
    }

    // --------------- 표시/숨김 ---------------

    public void SetVisible(bool on)
    {
        if (!cg) return;
        cg.alpha = on ? 1f : 0f;
        cg.interactable = on;
        cg.blocksRaycasts = on;

        Debug.Log($"[BattleHandUI] SetVisible({on}) → alpha={cg.alpha}, interactable={cg.interactable}, blocks={cg.blocksRaycasts}, active={gameObject.activeSelf}");
    }

    public bool IsVisible()
    {
        return cg && cg.alpha > 0.5f && cg.blocksRaycasts;
    }

    // --------------- 렌더링 ---------------

    public void Refresh()
    {
        if (!group || !cardUiPrefab) return;

        foreach (var c in spawned) if (c) Destroy(c.gameObject);
        spawned.Clear();

        var deck = BattleDeckRuntime.Instance;
        if (deck == null || deck.hand.Count == 0) return;

        int n = deck.hand.Count;
        float width = group.rect.width - horizontalPadding * 2f;
        float spacing = Mathf.Clamp(width / Mathf.Max(1, n - 1), minSpacing, maxSpacing);
        float startX = -0.5f * (spacing * (n - 1));

        for (int i = 0; i < n; i++)
        {
            string id = deck.hand[i];

            var go = Instantiate(cardUiPrefab, group);
            var ui = go.GetComponent<CardUI>() ?? go.AddComponent<CardUI>();

            var sp = Resources.Load<Sprite>($"{resourcesFolder}/{id}");
            ui.Init(this, i, sp);

            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = new Vector2(startX + spacing * i, 0f);
            rt.sizeDelta = new Vector2(cardWidth, rt.sizeDelta.y);

            spawned.Add(ui);
        }

        Debug.Log($"[BattleHandUI] Refresh 완료 → {spawned.Count}장");
    }

    // --------------- 카드 클릭 ---------------

    /// <summary>
    /// CardUI에서 호출(기존 호환용 이름).
    /// </summary>
    public void OnClickCard(int handIndex)
    {
        // 기존 코드 호환을 위해 남겨둔 래퍼 → 실제 처리로 위임
        OnCardClick(handIndex);
    }

    /// <summary>
    /// 실제 카드 클릭 처리.
    /// </summary>
    public void OnCardClick(int handIndex)
    {
        // 패널이 열려있지 않으면 무시(보호)
        if (!IsVisible()) return;

        if (TurnManager.Instance == null ||
            TurnManager.Instance.currentTurn != TurnState.PlayerTurn)
            return;

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

        // 패턴 읽기용 임시 GO
        var go = new GameObject($"_PlayerCard_{cardType.Name}");
        float total = 0f;

        try
        {
            var comp = go.AddComponent(cardType) as ICardPattern;
            if (comp == null) yield break;

            var timings = comp.Timings ?? new float[16];

            // 적 패널에 공격 표시
            attackController.ShowPattern(comp.Pattern16, timings, AttackController.Panel.Enemy);
            total = attackController.GetSequenceDuration(timings);
        }
        finally
        {
            Destroy(go);
        }

        // 사용 카드 → 덱 맨 아래
        BattleDeckRuntime.Instance.UseCardToBottom(handIndex);
        Refresh(); // 손패 즉시 반영(닫힌 상태에서도 내부 데이터 갱신)

        // 연출 대기 후 턴 종료
        if (total > 0f) yield return new WaitForSeconds(total);
        if (TurnManager.Instance) TurnManager.Instance.EndPlayerTurn();
    }

    // --------------- 유틸 ---------------

    private static Type FindTypeByName(string typeName)
    {
        var asm = typeof(BattleHandUI).Assembly;
        return asm.GetTypes().FirstOrDefault(
            t => t.Name == typeName && typeof(MonoBehaviour).IsAssignableFrom(t));
    }
}
