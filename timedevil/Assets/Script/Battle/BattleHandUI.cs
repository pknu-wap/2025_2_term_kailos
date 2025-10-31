using System.Collections.Generic;
using UnityEngine;

public class BattleHandUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private string resourcesFolder = "my_asset";
    [SerializeField] private AttackController attackController;

    [Header("Layout")]
    [SerializeField] private float cardWidth = 120f;
    [SerializeField] private float maxSpacing = 140f;
    [SerializeField] private float minSpacing = 40f;
    [SerializeField] private float horizontalPadding = 40f;

    private RectTransform group;
    private CanvasGroup cg;
    private readonly List<CardUI> cardUIs = new();

    private static int _lastSetFrame = -1;
    private static bool _lastSetState = false;

    void Awake()
    {
        EnsureRefs();
        SetVisible(false, "Awake");
        Debug.Log("[BattleHandUI] Awake() 초기화 완료");
    }

    private void EnsureRefs()
    {
        if (!group) group = (RectTransform)transform;
        if (!cg)
        {
            cg = GetComponent<CanvasGroup>();
            if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void SetVisible(bool on, string reason = "")
    {
        EnsureRefs();
        if (!cg)
        {
            Debug.LogWarning($"[BattleHandUI] CanvasGroup 없음 (reason={reason})");
            return;
        }

        if (_lastSetFrame == Time.frameCount && _lastSetState == on)
        {
            Debug.Log($"[BattleHandUI] 같은 프레임 중복호출 무시 (on={on}, reason={reason})");
            return;
        }
        _lastSetFrame = Time.frameCount;
        _lastSetState = on;

        cg.alpha = on ? 1f : 0f;
        cg.interactable = on;
        cg.blocksRaycasts = on;
        Debug.Log($"[BattleHandUI] SetVisible({on}) by {reason} → alpha={cg.alpha}, interactable={cg.interactable}, blocks={cg.blocksRaycasts}");
    }

    public void OnPlayerTurnStart()
    {
        Refresh();
    }

    public void OpenAndRefresh()
    {
        SetVisible(true, "OpenAndRefresh");
        Refresh();
    }

    public void Close()
    {
        SetVisible(false, "Close");
    }

    public void Refresh()
    {
        var bd = BattleDeckRuntime.Instance;
        if (bd == null)
        {
            Debug.LogWarning("[BattleHandUI] BattleDeckRuntime 없음 → Refresh 취소");
            return;
        }

        foreach (var ui in cardUIs)
            if (ui) Destroy(ui.gameObject);
        cardUIs.Clear();

        // 손패 생성
        for (int i = 0; i < bd.hand.Count; i++)
        {
            string id = bd.hand[i];
            var go = Instantiate(cardUIPrefab, group);
            var ui = go.GetComponent<CardUI>();
            var sprite = Resources.Load<Sprite>($"{resourcesFolder}/{id}");
            ui.Init(this, i, sprite);   // ★ 여기서 Init 사용
            cardUIs.Add(ui);
        }

        LayoutCards();
        Debug.Log($"[BattleHandUI] Refresh → {cardUIs.Count}장");
    }

    private void LayoutCards()
    {
        int count = cardUIs.Count;
        if (count == 0) return;

        float spacing = Mathf.Clamp(maxSpacing - count * 10, minSpacing, maxSpacing);
        float totalWidth = count * cardWidth + (count - 1) * spacing;
        float startX = -totalWidth / 2f + cardWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            float x = startX + i * (cardWidth + spacing);
            var rt = (RectTransform)cardUIs[i].transform;
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta = new Vector2(cardWidth, rt.sizeDelta.y);
        }
    }

    // ▼ CardUI에서 호출하는 콜백(간단 버전: 일단 로그만)
    public void OnClickCard(int handIndex)
    {
        Debug.Log($"[BattleHandUI] 카드 클릭: index={handIndex}");
        // 이후 기존 공격 로직(패턴 실행 → AttackController → 턴 종료) 연결할 때 여기서 진행하면 됨.
    }
}
