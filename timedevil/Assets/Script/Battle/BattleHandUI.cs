using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHandUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] RectTransform cardGroup;       // 손패를 깔아둘 부모(=card_group)
    [SerializeField] GameObject cardUIPrefab;       // CardUI 프리팹 (Image + Button)
    [SerializeField] string resourcesFolder = "my_asset";  // Resources/my_asset/<CardId>.png

    [Header("Layout")]
    [SerializeField] float cardWidth = 120f;        // 카드 1장의 가로 폭(대충)
    [SerializeField] float maxSpacing = 140f;       // 손패가 적을 때 카드 간격
    [SerializeField] float minSpacing = 40f;        // 손패가 많을 때 최소 간격(겹침 유도)
    [SerializeField] float horizontalPadding = 40f; // 좌우 여백

    readonly List<CardUI> spawned = new();

    void OnEnable()
    {
        Refresh();  // 열릴 때 갱신
    }

    public void Refresh()
    {
        // 기존 UI 정리
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) Destroy(spawned[i].gameObject);
        spawned.Clear();

        var bd = BattleDeckRuntime.Instance;
        if (bd == null || cardGroup == null || cardUIPrefab == null) return;

        List<string> hand = bd.hand;
        int n = hand.Count;
        if (n == 0) return;

        // 레이아웃 계산
        float groupWidth = cardGroup.rect.width - horizontalPadding * 2f;
        // 손패가 적으면 넓게, 많으면 간격을 줄여 겹치게
        float spacing;
        if (n <= 1) spacing = 0f;
        else
        {
            // groupWidth 안에 "첫 카드 left ~ 마지막 카드 right"가 들어오게
            // spacing을 계산 (spacing < cardWidth면 시각적으로 겹침)
            float ideal = Mathf.Min(maxSpacing, Mathf.Max(minSpacing, (groupWidth - cardWidth) / (n - 1)));
            spacing = ideal;
        }

        // 중앙 정렬: 전체 폭 = cardWidth + spacing*(n-1)
        float totalSpan = cardWidth + spacing * (n - 1);
        float startX = -totalSpan * 0.5f;

        for (int i = 0; i < n; i++)
        {
            string id = hand[i];
            var go = Instantiate(cardUIPrefab, cardGroup);
            var ui = go.GetComponent<CardUI>();
            if (!ui) ui = go.AddComponent<CardUI>();

            var sprite = Resources.Load<Sprite>($"{resourcesFolder}/{id}");
            ui.Setup(i, id, sprite, OnClickCard);

            // 위치 배치 (로컬 좌표 기준 수평 나열, 필요한 경우 Pivot이 Center인 프리팹 권장)
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = new Vector2(startX + i * spacing, 0f);
            rt.localScale = Vector3.one;

            spawned.Add(ui);
        }
    }

    void OnClickCard(int handIndex)
    {
        // 카드 사용 → 덱 맨 밑으로
        var bd = BattleDeckRuntime.Instance;
        if (bd != null && bd.UseCardToBottom(handIndex))
        {
            Refresh(); // 손패 갱신
            // 한 턴 1장 제한: 바로 턴 종료
            if (TurnManager.Instance != null)
                TurnManager.Instance.EndPlayerTurn();
        }
    }
}
