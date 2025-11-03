// Assets/Script/Battle/HandUI.cs
using System;
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

    // ----- Runtime -----
    private readonly List<GameObject> spawned = new();
    private readonly List<string> handIdsSnapshot = new();   // 화면에 보이는 손패 스냅샷
    public IReadOnlyList<string> VisibleHandIds => handIdsSnapshot;

    // 현재 데이터 소스(플레이어/적)
    private IHandReadable _source;

    private bool selecting = false;
    private int selectIndex = -1;

    public event Action<bool> onSelectModeChanged;
    public event Action<int> onSelectIndexChanged;

    public bool IsInSelectMode => selecting;
    public int CurrentSelectIndex => selectIndex;
    public int CardCount => handIdsSnapshot.Count;

    // ================== Unity ==================

    private void Awake()
    {
        if (!row) row = (RectTransform)transform;
        if (select) select.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // 기본 바인딩(없으면 플레이어 런타임)
        if (_source == null && BattleDeckRuntime.Instance != null)
            BindSource(BattleDeckRuntime.Instance);

        RebuildFromHand();
        SubscribeSource(true);
    }

    private void OnDisable()
    {
        SubscribeSource(false);
    }

    private void SubscribeSource(bool on)
    {
        if (_source == null) return;
        if (on) _source.OnHandChanged += RebuildFromHand;
        else _source.OnHandChanged -= RebuildFromHand;
    }

    // ================== Binding ==================

    public void BindToPlayer() => BindSource(BattleDeckRuntime.Instance);
    public void BindToEnemy() => BindSource(EnemyDeckRuntime.Instance);

    public void BindSource(IHandReadable source)
    {
        if (_source == source) return;
        SubscribeSource(false);
        _source = source;
        SubscribeSource(true);
        LogCaller("BindSource");   // 🔎 누가 바인딩 바꾸는지 확인
        RebuildFromHand();
    }

    // ================== Build / Refresh ==================

    public void RebuildFromHand()
    {
        if (!row || !cardPrefab || _source == null) return;

        // 스냅샷 생성
        handIdsSnapshot.Clear();
        var live = _source.GetHandIds();
        if (live != null) handIdsSnapshot.AddRange(live);

        // 기존 카드 UI 제거
        ClearSpawned();

        // 새로 생성 및 배치
        float x = leftPadding;
        for (int i = 0; i < handIdsSnapshot.Count; i++)
        {
            string id = handIdsSnapshot[i];
            var go = Instantiate(cardPrefab, row);
            go.name = $"HandCard_{id}";
            spawned.Add(go);

            var img = go.GetComponentInChildren<Image>() ?? go.AddComponent<Image>();
            img.sprite = !string.IsNullOrEmpty(id)
                ? Resources.Load<Sprite>($"{resourcesFolder}/{id}")
                : null;
            img.preserveAspect = true;
            img.raycastTarget = true;

            var rtItem = (RectTransform)go.transform;
            rtItem.anchorMin = rtItem.anchorMax = new Vector2(0f, 0.5f);
            rtItem.pivot = new Vector2(0f, 0.5f);
            rtItem.anchoredPosition = new Vector2(x, 0f);
            rtItem.sizeDelta = new Vector2(cardWidth, rtItem.sizeDelta.y);

            x += cardWidth;
        }

        // 손패가 바뀌면 선택 모드는 해제
        ExitSelectMode();

        LogCaller($"RebuildFromHand (cards={spawned.Count})");  // 🔎 누가 리빌드 유발하는지 확인
    }

    private void ClearSpawned()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) Destroy(spawned[i]);
        spawned.Clear();
    }

    // ================== Visibility helpers ==================

    /// <summary>모든 카드 게임오브젝트를 보이게.</summary>
    public void ShowCards()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) spawned[i].SetActive(true);

        LogCaller($"ShowCards (cards={spawned.Count}, selecting={selecting})"); // 🔎 콜스택
    }

    /// <summary>모든 카드 게임오브젝트를 숨김(선택 오버레이 포함). 선택 상태도 초기화.</summary>
    public void HideCards()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) spawned[i].SetActive(false);

        if (select) select.gameObject.SetActive(false);
        selecting = false;
        selectIndex = -1;

        LogCaller($"HideCards (cards={spawned.Count})"); // 🔎 콜스택
    }

    /// <summary>특정 슬롯의 이미지 on/off (오브젝트 비활성 아님).</summary>
    public void SetCardEnabled(int index, bool on)
    {
        if (index < 0 || index >= spawned.Count) return;
        var img = spawned[index] ? spawned[index].GetComponentInChildren<Image>() : null;
        if (img) img.enabled = on;

        // 참고 로그 (필요 없으면 주석)
        // LogCaller($"SetCardEnabled index={index} on={on}");
    }

    /// <summary>특정 슬롯 이미지를 다시 켠다.</summary>
    public void EnableCard(int index) => SetCardEnabled(index, true);

    /// <summary>모든 슬롯 이미지를 다시 켠다(프리뷰 이후 복구용).</summary>
    public void EnableAllCardImages()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            var img = spawned[i] ? spawned[i].GetComponentInChildren<Image>() : null;
            if (img) img.enabled = true;
        }
        // LogCaller("EnableAllCardImages");
    }

    // ================== Select mode (player) ==================

    public void EnterSelectMode()
    {
        if (CardCount == 0) return;
        selecting = true;

        if (select) select.gameObject.SetActive(true);
        onSelectModeChanged?.Invoke(true);

        // 기본 오른쪽 끝에서 시작
        SetSelectIndexPublic(CardCount - 1);

        LogCaller("EnterSelectMode");
    }

    public void ExitSelectMode()
    {
        if (!selecting) return;
        selecting = false;

        onSelectModeChanged?.Invoke(false);
        selectIndex = -1;

        if (select) select.gameObject.SetActive(false);

        LogCaller("ExitSelectMode");
    }

    public void MoveSelect(int delta)
    {
        if (!selecting || CardCount == 0) return;

        int next = selectIndex + delta;
        // 래핑
        next = (next % CardCount + CardCount) % CardCount;

        SetSelectIndexPublic(next);
    }

    public void SetSelectIndexPublic(int idx)
    {
        if (CardCount == 0) return;

        int prev = selectIndex;
        selectIndex = Mathf.Clamp(idx, 0, CardCount - 1);

        if (selectIndex != prev)
            onSelectIndexChanged?.Invoke(selectIndex);

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

    // ================== Helpers ==================

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

    // ================== Debug helper ==================

    // 에디터에서만 콜스택 출력 (빌드 용량/성능 방지)
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogCaller(string where)
    {
        var st = new System.Diagnostics.StackTrace(1, true); // 호출자부터
        Debug.Log($"[HandUI] {where} — cards={spawned.Count}, selecting={selecting}\n{st}");
    }
}
