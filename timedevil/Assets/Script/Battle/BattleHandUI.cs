using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHandUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] RectTransform cardGroup;       // ���и� ��Ƶ� �θ�(=card_group)
    [SerializeField] GameObject cardUIPrefab;       // CardUI ������ (Image + Button)
    [SerializeField] string resourcesFolder = "my_asset";  // Resources/my_asset/<CardId>.png

    [Header("Layout")]
    [SerializeField] float cardWidth = 120f;        // ī�� 1���� ���� ��(����)
    [SerializeField] float maxSpacing = 140f;       // ���а� ���� �� ī�� ����
    [SerializeField] float minSpacing = 40f;        // ���а� ���� �� �ּ� ����(��ħ ����)
    [SerializeField] float horizontalPadding = 40f; // �¿� ����

    readonly List<CardUI> spawned = new();

    void OnEnable()
    {
        Refresh();  // ���� �� ����
    }

    public void Refresh()
    {
        // ���� UI ����
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) Destroy(spawned[i].gameObject);
        spawned.Clear();

        var bd = BattleDeckRuntime.Instance;
        if (bd == null || cardGroup == null || cardUIPrefab == null) return;

        List<string> hand = bd.hand;
        int n = hand.Count;
        if (n == 0) return;

        // ���̾ƿ� ���
        float groupWidth = cardGroup.rect.width - horizontalPadding * 2f;
        // ���а� ������ �а�, ������ ������ �ٿ� ��ġ��
        float spacing;
        if (n <= 1) spacing = 0f;
        else
        {
            // groupWidth �ȿ� "ù ī�� left ~ ������ ī�� right"�� ������
            // spacing�� ��� (spacing < cardWidth�� �ð������� ��ħ)
            float ideal = Mathf.Min(maxSpacing, Mathf.Max(minSpacing, (groupWidth - cardWidth) / (n - 1)));
            spacing = ideal;
        }

        // �߾� ����: ��ü �� = cardWidth + spacing*(n-1)
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

            // ��ġ ��ġ (���� ��ǥ ���� ���� ����, �ʿ��� ��� Pivot�� Center�� ������ ����)
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = new Vector2(startX + i * spacing, 0f);
            rt.localScale = Vector3.one;

            spawned.Add(ui);
        }
    }

    void OnClickCard(int handIndex)
    {
        // ī�� ��� �� �� �� ������
        var bd = BattleDeckRuntime.Instance;
        if (bd != null && bd.UseCardToBottom(handIndex))
        {
            Refresh(); // ���� ����
            // �� �� 1�� ����: �ٷ� �� ����
            if (TurnManager.Instance != null)
                TurnManager.Instance.EndPlayerTurn();
        }
    }
}
