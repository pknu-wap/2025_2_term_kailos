using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform row;        // Hand 패널(자기 자신이면 비워도 됨)
    [SerializeField] private GameObject cardPrefab;    // Image 1개 이상 포함된 간단 프리팹
    [SerializeField] private string resourcesFolder = "my_asset";

    [Header("Layout (single row, left aligned)")]
    [SerializeField] private float leftPadding = 8f;
    [SerializeField] private float cardWidth = 120f; // 붙여서 배치하므로 간격=폭

    private readonly List<GameObject> spawned = new();

    void Awake()
    {
        if (!row) row = (RectTransform)transform;
        // 시작 시엔 카드 이미지만 숨겨둔다
        HideCards();
    }

    void OnEnable()
    {
        if (BattleDeckRuntime.Instance != null)
            BattleDeckRuntime.Instance.OnHandChanged += RebuildFromHand;
        // 씬 시작 직후 손패가 이미 있을 수 있으니 한 번 맞춰두기
        RebuildFromHand();
    }

    void OnDisable()
    {
        if (BattleDeckRuntime.Instance != null)
            BattleDeckRuntime.Instance.OnHandChanged -= RebuildFromHand;
    }

    /// <summary>현재 손패(BattleDeckRuntime.hand) 기준으로 카드 아이콘을 싹 갈아끼움.</summary>
    public void RebuildFromHand()
    {
        if (!row) row = (RectTransform)transform;

        // 프리팹 없으면 아무 것도 하지 않음
        if (!cardPrefab) return;

        var rt = BattleDeckRuntime.Instance;
        if (rt == null) return;

        var hand = rt.GetHandIds();
        ClearSpawned();

        // 왼쪽 붙여서 배치
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

            x += cardWidth; // 간격 없이 바로 붙이기
        }
    }

    /// <summary>런타임으로 생성했던 카드 오브젝트 전부 제거.</summary>
    private void ClearSpawned()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) Destroy(spawned[i]);
        spawned.Clear();
    }

    /// <summary>생성된 카드들만 보이기.</summary>
    public void ShowCards()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) spawned[i].SetActive(true);
    }

    /// <summary>생성된 카드들만 숨기기(Hand 오브젝트는 계속 켜둠).</summary>
    public void HideCards()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) spawned[i].SetActive(false);
    }
}
