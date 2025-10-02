using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleHandUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform cardGroup;   // card_group
    [SerializeField] private GameObject cardUiPrefab;
    [SerializeField] private string resourcesFolder = "my_asset";
    [SerializeField] private AttackController attackController;

    [Header("Layout")]
    [SerializeField] private float cardWidth = 120f;
    [SerializeField] private float maxSpacing = 140f;
    [SerializeField] private float minSpacing = 40f;
    [SerializeField] private float horizontalPadding = 40f;

    readonly List<CardUI> spawned = new();
    bool usedThisTurn = false;

    void Start()
    {
        Refresh(); // 시작 시 데이터만 반영 (표시는 TurnManager가 제어)
    }

    // TurnManager가 호출
    public void OnPlayerTurnStart()
    {
        usedThisTurn = false;
        BattleDeckRuntime.Instance?.DrawOneIfNeeded();
        Refresh();                  // 데이터 갱신
        // SetVisible(false)는 TurnManager가 호출
    }

    public void SetVisible(bool on)
    {
        if (cardGroup) cardGroup.gameObject.SetActive(on);
    }

    public void ToggleVisible()
    {
        if (!cardGroup) return;
        cardGroup.gameObject.SetActive(!cardGroup.gameObject.activeSelf);
        if (cardGroup.gameObject.activeSelf) Refresh(); // 열릴 때 새로 그림
    }

    public void Refresh()
    {
        if (cardGroup == null || cardUiPrefab == null) return;

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
            var sp = Resources.Load<Sprite>($"{resourcesFolder}/{id}");
            ui.Init(this, i, sp);

            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = new Vector2(startX + spacing * i, 0f);
            rt.sizeDelta = new Vector2(cardWidth, rt.sizeDelta.y);

            spawned.Add(ui);
        }
    }

    public void OnClickCard(int handIndex)
    {
        Debug.Log($"[HandUI] OnClickCard index={handIndex}");
        if (TurnManager.Instance == null || TurnManager.Instance.currentTurn != TurnState.PlayerTurn) return;
        if (usedThisTurn) return;

        var deck = BattleDeckRuntime.Instance;
        if (deck == null || handIndex < 0 || handIndex >= deck.hand.Count) return;

        string id = deck.hand[handIndex];
        var t = FindTypeByName(id);
        if (t == null) { Debug.LogWarning($"[HandUI] 카드 타입 없음: {id}"); return; }

        StartCoroutine(Co_UseCardAndAttack(handIndex, t));
    }

    System.Collections.IEnumerator Co_UseCardAndAttack(int handIndex, Type cardType)
    {
        usedThisTurn = true;
        SetVisible(false);

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
        finally { Destroy(go); }

        BattleDeckRuntime.Instance.UseCardToBottom(handIndex);
        Refresh();

        if (total > 0f) yield return new WaitForSeconds(total);

        // 턴 종료 → TurnManager가 여기서 UI를 닫음
        TurnManager.Instance.EndPlayerTurn();
    }

    static Type FindTypeByName(string typeName)
    {
        var asm = typeof(BattleHandUI).Assembly;
        return asm.GetTypes().FirstOrDefault(t => t.Name == typeName && typeof(MonoBehaviour).IsAssignableFrom(t));
    }
}
