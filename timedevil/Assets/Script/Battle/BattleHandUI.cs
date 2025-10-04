// BattleHandUI.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]  // ← 에디터에서 미리 붙여둡니다.
public class BattleHandUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject cardUiPrefab;
    [SerializeField] private string resourcesFolder = "my_asset";
    [SerializeField] private AttackController attackController;

    [Header("Layout")]
    [SerializeField] private float cardWidth = 120f;
    [SerializeField] private float maxSpacing = 140f;
    [SerializeField] private float minSpacing = 40f;
    [SerializeField] private float horizontalPadding = 40f;

    private readonly List<CardUI> spawned = new();
    private bool usedThisTurn = false;

    CanvasGroup cg;
    RectTransform group;

    // 같은 프레임 재닫힘 방지용
    int lastSetVisibleFrame = -9999;

    void Awake()
    {
        group = (RectTransform)transform;
        cg = GetComponent<CanvasGroup>(); // RequireComponent 덕분에 반드시 있음
        // 시작은 닫힘
        SetVisible(false, reason: "Awake");
        Debug.Log("[BattleHandUI] Awake 완료");
    }

    public void OnPlayerTurnStart()
    {
        usedThisTurn = false;
        BattleDeckRuntime.Instance?.DrawOneIfNeeded();
        Refresh();
        SetVisible(false, reason: "OnPlayerTurnStart");
    }

    public void OpenAndRefresh()
    {
        SetVisible(true, reason: "OpenAndRefresh");
        Refresh();
    }

    public void Close() => SetVisible(false, reason: "Close");

    public void SetVisible(bool on, string reason = "")
    {
        // 강제 활성화 가드: 보여줄 때는 비활성화여도 켠다.
        if (on && !gameObject.activeSelf)
            gameObject.SetActive(true);

        // 같은 프레임 재호출 보호: Hide→Show 충돌 방지
        if (Time.frameCount == lastSetVisibleFrame)
        {
            Debug.Log($"[BattleHandUI] 같은 프레임 재호출 무시 (on={on}, by={reason})");
            return;
        }
        lastSetVisibleFrame = Time.frameCount;

        // CanvasGroup 업데이트
        cg.alpha = on ? 1f : 0f;
        cg.interactable = on;
        cg.blocksRaycasts = on;

        // 절대 SetActive(false) 하지 않음(비가시화는 CanvasGroup만으로)
        Debug.Log($"[BattleHandUI] SetVisible({on}) by {reason} → alpha={cg.alpha}, interactable={cg.interactable}, blocks={cg.blocksRaycasts}, active={gameObject.activeSelf}, frame={Time.frameCount}");
    }

    public bool IsVisible() => cg && cg.alpha > 0.5f && cg.blocksRaycasts;

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

        Debug.Log($"[BattleHandUI] Refresh → {spawned.Count}장");
    }

    public void OnClickCard(int handIndex) => OnCardClick(handIndex);

    public void OnCardClick(int handIndex)
    {
        if (!IsVisible()) return;
        if (TurnManager.Instance == null || TurnManager.Instance.currentTurn != TurnState.PlayerTurn) return;
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
        SetVisible(false, reason: "UseCard"); // 입력 차단 & 숨김

        var go = new GameObject($"_PlayerCard_{cardType.Name}");
        float total = 0f;

        try
        {
            var comp = go.AddComponent(cardType) as ICardPattern;
            if (comp == null) yield break;
            var timings = comp.Timings ?? new float[16];

            attackController.ShowPattern(comp.Pattern16, timings, AttackController.Panel.Enemy);
            total = attackController.GetSequenceDuration(timings);
        }
        finally
        {
            Destroy(go);
        }

        BattleDeckRuntime.Instance.UseCardToBottom(handIndex);
        Refresh();

        if (total > 0f) yield return new WaitForSeconds(total);
        if (TurnManager.Instance) TurnManager.Instance.EndPlayerTurn();
    }

    private static Type FindTypeByName(string typeName)
    {
        var asm = typeof(BattleHandUI).Assembly;
        return asm.GetTypes().FirstOrDefault(
            t => t.Name == typeName && typeof(MonoBehaviour).IsAssignableFrom(t));
    }
}
