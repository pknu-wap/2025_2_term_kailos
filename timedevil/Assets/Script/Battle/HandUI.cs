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
        float x = leftPadding;
        for (int i = 0; i < handIdsSnapshot.Count; i++)
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
            rtItem.anchoredPosition = new Vector2(x, 0f);
            rtItem.sizeDelta = new Vector2(cardWidth, rtItem.sizeDelta.y);

            x += cardWidth;
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
}
