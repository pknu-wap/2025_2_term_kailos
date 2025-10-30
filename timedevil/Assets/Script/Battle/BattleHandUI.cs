// BattleHandUI.cs (업데이트 버전)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHandUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform handArea;      // CardPanel RectTransform
    [SerializeField] private GameObject cardUIPrefab;     // 루트에 Image가 있는 간단 프리팹이면 OK
    [SerializeField] private string resourcesFolder = "my_asset"; // Resources 폴더 경로

    [Header("Layout")]
    [SerializeField] private float cardWidth = 120f;
    [SerializeField] private float minSpacing = 40f;
    [SerializeField] private float horizontalPadding = 40f;

    [Header("Visibility")]
    [SerializeField] private CanvasGroup canvasGroup;     // CardPanel의 CanvasGroup (없어도 동작)

    private readonly List<GameObject> spawned = new();

    void Awake()
    {
        if (!handArea) handArea = GetComponent<RectTransform>();
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>(); // 없으면 null 허용
        Show(true); // 시작 기본: 보이게 (Run일 때만 숨김)
    }

    // BattleMenuController.onFocusChanged(int)에 연결
    public void OnMenuFocusChanged(int index)
    {
        Debug.Log($"[BattleHandUI] Focus index={index}");
        // Run(2)에서만 숨김, 그 외엔 항상 보이기(요구사항)
        bool visible = (index != 2);
        Show(visible);

        // Card(0)로 포커스 오면 손패 그리기
        if (index == 0)
            PaintHandFromRuntime(); 
    }

    private void Show(bool on)
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = on ? 1f : 0f;
            canvasGroup.blocksRaycasts = on;
            canvasGroup.interactable = on;
        }
        // CanvasGroup이 없어도 Panel 자체가 Active면 보임
        if (handArea) handArea.gameObject.SetActive(on);
    }

    /// <summary>CardStateRuntime/ BattleDeckRuntime 에서 현재 손패 그대로 그리기</summary>
    public void PaintHandFromRuntime()
    {
        // 손패 가져오기
        var deckRt = BattleDeckRuntime.Instance;
        if (deckRt == null)
        {
            Debug.LogWarning("[BattleHandUI] BattleDeckRuntime.Instance가 없음");
            return;
        }

        List<string> hand = deckRt.hand; // 이미 초기 드로우 된 손패

        // 기존 제거
        for (int i = spawned.Count - 1; i >= 0; i--)
            if (spawned[i]) Destroy(spawned[i]);
        spawned.Clear();

        if (hand == null || hand.Count == 0)
        {
            Debug.Log("[BattleHandUI] 손패가 비어 있음");
            return;
        }

        float x = horizontalPadding;
        for (int i = 0; i < hand.Count; i++)
        {
            string id = hand[i];

            // 생성
            var go = Instantiate(cardUIPrefab, handArea);
            go.name = $"Card_{id}";

            // ★ 반드시 켠다 (프리팹이 꺼져 있어도 강제 활성화)
            if (!go.activeSelf) go.SetActive(true);

            spawned.Add(go);

            // 이미지 스프라이트 세팅
            var img = go.GetComponent<Image>();
            if (img != null)
            {
                var sp = Resources.Load<Sprite>($"{resourcesFolder}/{id}");
                if (sp == null)
                    Debug.LogWarning($"[BattleHandUI] Sprite not found: {resourcesFolder}/{id}");
                img.sprite = sp;
                img.preserveAspect = true;
                img.enabled = true; // 혹시 비활성화되어 저장된 경우 대비
            }

            // 배치
            var rt = go.GetComponent<RectTransform>();
            if (rt)
            {
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.sizeDelta = new Vector2(cardWidth, rt.sizeDelta.y);
                rt.anchoredPosition = new Vector2(x, 0f);
            }

            x += cardWidth + minSpacing;
        }

        Debug.Log($"[BattleHandUI] Painthand -> [{string.Join(", ", hand)}], spawned={spawned.Count}");
    }
}
