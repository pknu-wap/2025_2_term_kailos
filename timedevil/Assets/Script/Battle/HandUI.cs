using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform row;        // Hand �г�(�ڱ� �ڽ��̸� ����� ��)
    [SerializeField] private GameObject cardPrefab;    // Image 1�� �̻� ���Ե� ���� ������
    [SerializeField] private string resourcesFolder = "my_asset";

    [Header("Layout (single row, left aligned)")]
    [SerializeField] private float leftPadding = 8f;
    [SerializeField] private float cardWidth = 120f; // �ٿ��� ��ġ�ϹǷ� ����=��

    private readonly List<GameObject> spawned = new();

    void Awake()
    {
        if (!row) row = (RectTransform)transform;
        // ���� �ÿ� ī�� �̹����� ���ܵд�
        HideCards();
    }

    void OnEnable()
    {
        if (BattleDeckRuntime.Instance != null)
            BattleDeckRuntime.Instance.OnHandChanged += RebuildFromHand;
        // �� ���� ���� ���а� �̹� ���� �� ������ �� �� ����α�
        RebuildFromHand();
    }

    void OnDisable()
    {
        if (BattleDeckRuntime.Instance != null)
            BattleDeckRuntime.Instance.OnHandChanged -= RebuildFromHand;
    }

    /// <summary>���� ����(BattleDeckRuntime.hand) �������� ī�� �������� �� ���Ƴ���.</summary>
    public void RebuildFromHand()
    {
        if (!row) row = (RectTransform)transform;

        // ������ ������ �ƹ� �͵� ���� ����
        if (!cardPrefab) return;

        var rt = BattleDeckRuntime.Instance;
        if (rt == null) return;

        var hand = rt.GetHandIds();
        ClearSpawned();

        // ���� �ٿ��� ��ġ
        float x = leftPadding;
        for (int i = 0; i < hand.Count; i++)
        {
            string id = hand[i];
            var go = Instantiate(cardPrefab, row);
            go.name = $"HandCard_{id}";
            spawned.Add(go);

            var img = go.GetComponentInChildren<Image>();
            if (!img) img = go.AddComponent<Image>();

            Sprite s = null;
            if (!string.IsNullOrEmpty(id))
                s = Resources.Load<Sprite>($"{resourcesFolder}/{id}");
            img.sprite = s;
            img.preserveAspect = true;
            img.raycastTarget = true;

            var itemRT = (RectTransform)go.transform;
            itemRT.anchorMin = itemRT.anchorMax = new Vector2(0f, 0.5f);
            itemRT.pivot = new Vector2(0f, 0.5f);
            itemRT.anchoredPosition = new Vector2(x, 0f);
            itemRT.sizeDelta = new Vector2(cardWidth, itemRT.sizeDelta.y);

            x += cardWidth; // ���� ���� �ٷ� ���̱�
        }
    }

    /// <summary>��Ÿ������ �����ߴ� ī�� ������Ʈ ���� ����.</summary>
    private void ClearSpawned()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) Destroy(spawned[i]);
        spawned.Clear();
    }

    /// <summary>������ ī��鸸 ���̱�.</summary>
    public void ShowCards()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) spawned[i].SetActive(true);
    }

    /// <summary>������ ī��鸸 �����(Hand ������Ʈ�� ��� �ѵ�).</summary>
    public void HideCards()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) spawned[i].SetActive(false);
    }
}
