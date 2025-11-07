using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform row;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private string resourcesFolder = "my_asset";

    [Header("Layout (single row, left aligned)")]
    [SerializeField] private float leftPadding = 8f;
    [SerializeField] private float rightPadding = 8f; // 👈 추가: 오른쪽 여백

    [SerializeField] private float cardWidth = 120f;

    [Header("Select Overlay")]
    [SerializeField] private RectTransform select;
    [SerializeField] private Vector2 selectPadding = new Vector2(8f, 8f);

    private readonly List<GameObject> spawned = new();
    private readonly List<string> handIdsSnapshot = new();
    public IReadOnlyList<string> VisibleHandIds => handIdsSnapshot;

    private bool selecting = false;
    private int selectIndex = -1;

    public event System.Action<bool> onSelectModeChanged;
    public event System.Action<int> onSelectIndexChanged;

    public bool IsInSelectMode => selecting;
    public int CurrentSelectIndex => selectIndex;
    public int CardCount => handIdsSnapshot.Count;

    void Awake()
    {
        if (!row) row = (RectTransform)transform;
        HideCards();
        if (select) select.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (BattleDeckRuntime.Instance != null)
            BattleDeckRuntime.Instance.OnHandChanged += RebuildFromHand;

        RebuildFromHand();
    }

    void OnDisable()
    {
        if (BattleDeckRuntime.Instance != null)
            BattleDeckRuntime.Instance.OnHandChanged -= RebuildFromHand;
    }

    // 입력은 여기서 처리하지 않음

    public void RebuildFromHand()
    {
        if (!row) row = (RectTransform)transform;
        if (!cardPrefab) return;
        var rt = BattleDeckRuntime.Instance;
        if (rt == null) return;

        handIdsSnapshot.Clear();
        var live = rt.GetHandIds();
        if (live != null) handIdsSnapshot.AddRange(live);

        ClearSpawned();
        // --- 배치 계산: 패널 너비 안에서 첫/끝 카드가 항상 들어오도록 step 계산 ---
        float rowW = row.rect.width;
        // 사용할 수 있는 가로폭
        float usable = Mathf.Max(0f, rowW - leftPadding - rightPadding);

        int n = handIdsSnapshot.Count;

        // n==0이면 아래 루프 자체가 돌지 않지만 안전하게 초기화
        float step = 0f;
        if (n <= 1)
        {
            step = 0f; // 한 장이면 패널 안 왼쪽에 그대로
        }
        else
        {
            // 마지막 카드의 오른쪽 끝이 패널을 넘지 않도록:
            // 첫 카드 x=leftPadding, 마지막 카드 x=leftPadding + step*(n-1)
            // 마지막 카드의 "오른쪽 끝" = 그 x + cardWidth <= leftPadding + usable
            // => step*(n-1) <= usable - cardWidth
            float maxSpan = Mathf.Max(0f, usable - cardWidth);
            float needed = maxSpan / (n - 1);

            // 카드 크기는 유지, 간격만 줄이기(겹치기 허용). 간격의 상한은 cardWidth.
            step = Mathf.Min(cardWidth, Mathf.Max(0f, needed));
        }

        // --- 스폰 & 배치 ---
        ClearSpawned();
        for (int i = 0; i < n; i++)
        {
            string id = handIdsSnapshot[i];
            var go = Instantiate(cardPrefab, row);
            go.name = $"HandCard_{id}";
            spawned.Add(go);

            var img = go.GetComponentInChildren<Image>() ?? go.AddComponent<Image>();
            img.sprite = !string.IsNullOrEmpty(id) ? Resources.Load<Sprite>($"{resourcesFolder}/{id}") : null;
            img.preserveAspect = true;
            img.raycastTarget = true;

            var rtItem = (RectTransform)go.transform;
            rtItem.anchorMin = rtItem.anchorMax = new Vector2(0f, 0.5f);
            rtItem.pivot = new Vector2(0f, 0.5f);

            // 계산된 step으로 겹치기/간격 적용
            float x = leftPadding + step * i;
            rtItem.anchoredPosition = new Vector2(x, 0f);

            // 카드 자체 크기는 그대로 유지
            rtItem.sizeDelta = new Vector2(cardWidth, rtItem.sizeDelta.y);
        }

        // 손패 변경 시 선택 해제되더라도, 다음 진입에서 회색 방지
        ExitSelectMode();
        ShowCards(); // ✅ 항상 켜 두기 (중요)
    }

    private void ClearSpawned()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) Destroy(spawned[i]);
        spawned.Clear();
    }

    public void ShowCards()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) spawned[i].SetActive(true);
    }

    public void HideCards()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) spawned[i].SetActive(false);
        if (select) select.gameObject.SetActive(false);
        selecting = false;
        selectIndex = -1;
    }

    // ---- 선택모드 공개 API ----
    public void EnterSelectMode()
    {
        if (CardCount == 0) return;

        ShowCards();                 // ✅ 재진입 시 반드시 on
        selecting = true;
        if (select) select.gameObject.SetActive(true);
        onSelectModeChanged?.Invoke(true);

        SetSelectIndexPublic(CardCount - 1); // 오른쪽 끝부터
    }

    public void ExitSelectMode()
    {
        if (!selecting) return;
        selecting = false;
        onSelectModeChanged?.Invoke(false);
        selectIndex = -1;
        if (select) select.gameObject.SetActive(false);
    }

    public void MoveSelect(int delta)
    {
        if (!selecting || CardCount == 0) return;
        int next = selectIndex + delta;
        next = (next % CardCount + CardCount) % CardCount; // 래핑
        SetSelectIndexPublic(next);
    }

    public void SetSelectIndexPublic(int idx)
    {
        if (CardCount == 0) return;

        int prev = selectIndex;
        selectIndex = Mathf.Clamp(idx, 0, CardCount - 1);
        if (selectIndex != prev) onSelectIndexChanged?.Invoke(selectIndex);

        if (select && selectIndex >= 0 && selectIndex < spawned.Count)
        {
            var target = (RectTransform)spawned[selectIndex].transform;
            select.SetParent(row, false);
            select.anchorMin = select.anchorMax = new Vector2(0f, 0.5f);
            select.pivot = new Vector2(0f, 0.5f);

            var size = target.sizeDelta + selectPadding * 2f;
            var pos = target.anchoredPosition - new Vector2(selectPadding.x, 0f);

            select.sizeDelta = new Vector2(size.x, Mathf.Max(size.y, 0f));
            select.anchoredPosition = new Vector2(pos.x, 0f);
            select.SetAsLastSibling();
        }
    }

    public RectTransform GetCardRect(int index)
    {
        if (index < 0 || index >= spawned.Count || !spawned[index]) return null;
        return (RectTransform)spawned[index].transform;
    }

    public string GetVisibleIdAt(int index)
    {
        if (index < 0 || index >= handIdsSnapshot.Count) return null;
        return handIdsSnapshot[index];
    }

    public List<RectTransform> GetAllCardRects()
    {
        var list = new List<RectTransform>();
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) list.Add((RectTransform)spawned[i].transform);
        return list;
    }
}
