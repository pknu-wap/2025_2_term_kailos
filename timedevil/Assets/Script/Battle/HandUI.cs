using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform row;        // Hand 컨테이너
    [SerializeField] private GameObject cardPrefab;    // 단순 Image 프리팹
    [SerializeField] private string resourcesFolder = "my_asset";

    [Header("Layout (single row, left aligned)")]
    [SerializeField] private float leftPadding = 8f;
    [SerializeField] private float cardWidth = 120f; // 붙여서 배치

    [Header("Select Overlay")]
    [SerializeField] private RectTransform select;     // ✅ 주황 프레임 이미지
    [SerializeField] private Vector2 selectPadding = new Vector2(8f, 8f);

    private readonly List<GameObject> spawned = new();

    // 선택 모드 상태
    private bool selecting = false;
    private int selectIndex = -1;

    void Awake()
    {
        if (!row) row = (RectTransform)transform;
        HideCards();
        if (select) select.gameObject.SetActive(false);   // 처음엔 숨김
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

    void Update()
    {
        if (!selecting) return;

        // 선택모드일 때만 이동/취소 입력을 받음
        int count = spawned.Count;
        if (count == 0) { ExitSelectMode(); return; }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            SetSelectIndex((selectIndex - 1 + count) % count);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            SetSelectIndex((selectIndex + 1) % count);
        else if (Input.GetKeyDown(KeyCode.Q))
            ExitSelectMode(); // 선택 취소
    }

    /// <summary>현재 손패 기준으로 카드 아이콘 전부 재생성 + 왼쪽부터 붙여 배치.</summary>
    public void RebuildFromHand()
    {
        if (!row) row = (RectTransform)transform;
        if (!cardPrefab) return;

        var rt = BattleDeckRuntime.Instance;
        if (rt == null) return;

        var hand = rt.GetHandIds();
        ClearSpawned();

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

            x += cardWidth;
        }

        // 손패가 바뀌면 선택모드는 해제(안전하게)
        ExitSelectMode();
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
        if (select) select.gameObject.SetActive(false); // 선택 프레임도 함께 숨김
        selecting = false;
        selectIndex = -1;
    }

    // ---------- 선택 모드 ----------

    /// <summary>Card가 E로 선택됐을 때 호출 → 선택모드 진입, 오른쪽 끝 카드부터.</summary>
    public void EnterSelectMode()
    {
        int count = spawned.Count;
        if (count == 0 || !select) return;

        selecting = true;
        select.gameObject.SetActive(true);
        // 오른쪽 끝 카드(가장 마지막)
        SetSelectIndex(count - 1);
    }

    /// <summary>선택모드 종료(취소/Q).</summary>
    public void ExitSelectMode()
    {
        selecting = false;
        selectIndex = -1;
        if (select) select.gameObject.SetActive(false);
    }

    private void SetSelectIndex(int idx)
    {
        if (spawned.Count == 0 || !select) return;

        selectIndex = Mathf.Clamp(idx, 0, spawned.Count - 1);
        var target = (RectTransform)spawned[selectIndex].transform;

        // select를 Hand 좌표계에서 타겟 위치/크기에 맞추기
        select.SetParent(row, worldPositionStays: false);
        select.anchorMin = select.anchorMax = new Vector2(0f, 0.5f);
        select.pivot = new Vector2(0f, 0.5f);

        // 패딩을 준 테두리로 보이게
        var size = target.sizeDelta + selectPadding * 2f;
        var pos = target.anchoredPosition - new Vector2(selectPadding.x, 0f);

        select.sizeDelta = new Vector2(size.x, Mathf.Max(size.y, 0f));
        select.anchoredPosition = new Vector2(pos.x, 0f);

        // 겹칠 가능성 대비 맨 위로
        select.SetAsLastSibling();
    }

    /// <summary>현재 선택모드 중인지 확인.</summary>
    public bool IsSelecting() => selecting;
    public int CardCount => spawned.Count;


    public RectTransform GetCardRect(int index)
    {
        if (index < 0 || index >= spawned.Count || !spawned[index]) return null;
        return (RectTransform)spawned[index].transform;
    }
}
