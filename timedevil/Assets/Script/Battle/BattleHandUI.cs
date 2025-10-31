// BattleHandUI.cs (업데이트 버전)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHandUI : MonoBehaviour
{
    [Header("Refs")]
<<<<<<< Updated upstream
    [SerializeField] private RectTransform handArea;      // CardPanel RectTransform
    [SerializeField] private GameObject cardUIPrefab;     // 루트에 Image가 있는 간단 프리팹이면 OK
    [SerializeField] private string resourcesFolder = "my_asset"; // Resources 폴더 경로
=======
<<<<<<< HEAD
    [SerializeField] private GameObject cardUIPrefab;     // 씬에 있는 Card 프리팹(템플릿)
    [SerializeField] private string resourcesFolder = "my_asset";
    [SerializeField] private AttackController attackController;
=======
    [SerializeField] private RectTransform handArea;      // CardPanel RectTransform
    [SerializeField] private GameObject cardUIPrefab;     // 루트에 Image가 있는 간단 프리팹이면 OK
    [SerializeField] private string resourcesFolder = "my_asset"; // Resources 폴더 경로
>>>>>>> ba18b172bfd51ffca4970fd4549e0728dd6d1a79
>>>>>>> Stashed changes

    [Header("Layout")]
    [SerializeField] private float cardWidth = 120f;
    [SerializeField] private float minSpacing = 40f;
    [SerializeField] private float horizontalPadding = 40f;

<<<<<<< Updated upstream
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
=======
<<<<<<< HEAD
    private RectTransform group;

    // 여기에는 "런타임에 생성한 카드들"만 담는다 (씬에 있던 템플릿 Card는 포함 ❌)
    private readonly List<CardUI> spawnedCards = new();

    void Awake()
    {
        EnsureRefs();

        // ▶ 씬에 미리 놓아둔 Card 템플릿(하얀 사각형)은 시작 시 바로 꺼준다.
        DeactivateSceneTemplates();

        // 시작 화면은 카드가 안 보이는 상태여야 하므로,
        // 혹시 에디터에서 테스트로 남아있는 생성분이 있었다면 꺼두기(안전망).
        HideCards();
=======
    [Header("Visibility")]
    [SerializeField] private CanvasGroup canvasGroup;     // CardPanel의 CanvasGroup (없어도 동작)

    private readonly List<GameObject> spawned = new();

    void Awake()
    {
        if (!handArea) handArea = GetComponent<RectTransform>();
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>(); // 없으면 null 허용
        Show(true); // 시작 기본: 보이게 (Run일 때만 숨김)
>>>>>>> ba18b172bfd51ffca4970fd4549e0728dd6d1a79
    }

    // BattleMenuController.onFocusChanged(int)에 연결
    public void OnMenuFocusChanged(int index)
    {
<<<<<<< HEAD
        if (!group) group = (RectTransform)transform;
    }

    /// <summary>
    /// Hand 하위 자식들 중에서 CardUI가 없는(=씬에 배치한 템플릿) 오브젝트는 전부 비활성화.
    /// </summary>
    private void DeactivateSceneTemplates()
    {
        for (int i = 0; i < group.childCount; i++)
        {
            var child = group.GetChild(i).gameObject;
            if (!child.GetComponent<CardUI>())
            {
                // 템플릿으로 판단 → 꺼둔다
                child.SetActive(false);
            }
=======
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
>>>>>>> ba18b172bfd51ffca4970fd4549e0728dd6d1a79
        }
        // CanvasGroup이 없어도 Panel 자체가 Active면 보임
        if (handArea) handArea.gameObject.SetActive(on);
    }

<<<<<<< HEAD
    /// <summary>
    /// 현재 손패(BattleDeckRuntime.Instance.hand)로부터 동기화해서
    /// 기존 생성 카드들을 지우고 새로 생성한 뒤 '보이게' 만든다.
    /// </summary>
    public void SyncFromDeckAndShow()
    {
        EnsureRefs();

        var bd = BattleDeckRuntime.Instance;
        if (!cardUIPrefab || bd == null || bd.hand == null)
        {
            Debug.LogWarning("[BattleHandUI] 동기화 불가: 프리팹/덱이 없습니다.");
            return;
        }

        // 씬 템플릿은 유지, 이전에 "생성했던" 카드들만 제거
        for (int i = 0; i < spawnedCards.Count; i++)
            if (spawnedCards[i]) Destroy(spawnedCards[i].gameObject);
        spawnedCards.Clear();

        // 손패 기반으로 새로 생성
        for (int i = 0; i < bd.hand.Count; i++)
        {
            string id = bd.hand[i];

            var go = Instantiate(cardUIPrefab, group);
            go.name = $"Card_{id}";
            go.SetActive(true); // 템플릿에서 복제된 프리팹은 켜서 사용

            var ui = go.GetComponent<CardUI>();
            if (!ui) ui = go.AddComponent<CardUI>();

            // 아트 이미지(없으면 자동 추가)
            var art = ui.GetComponentInChildren<Image>();
            if (!art) art = go.AddComponent<Image>();

            // 스프라이트 로드
            Sprite sprite = null;
            if (!string.IsNullOrEmpty(id))
            {
                var path = $"{resourcesFolder}/{id}";
                sprite = Resources.Load<Sprite>(path);
                if (!sprite) Debug.LogWarning($"[BattleHandUI] 스프라이트 못찾음: {path}");
            }

            ui.Init(this, i, sprite);
            spawnedCards.Add(ui);
        }

        LayoutCards();
        ShowCards(); // Card 포커스에서 호출되므로 생성 즉시 보이게
        Debug.Log($"[BattleHandUI] SyncFromDeckAndShow → {spawnedCards.Count}장 재배치");
    }

    private void LayoutCards()
    {
        int count = spawnedCards.Count;
        if (count == 0) return;

        float spacing = Mathf.Clamp(maxSpacing - count * 10, minSpacing, maxSpacing);
        float totalWidth = count * cardWidth + (count - 1) * spacing;
        float startX = -totalWidth / 2f + cardWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            float x = startX + i * (cardWidth + spacing);
            var rt = (RectTransform)spawnedCards[i].transform;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta = new Vector2(cardWidth, rt.sizeDelta.y);
        }
    }

    /// <summary>
    /// 생성된 카드들만 보이게(Hand 오브젝트/씬 템플릿은 그대로)
    /// </summary>
    public void ShowCards()
    {
        for (int i = 0; i < spawnedCards.Count; i++)
            if (spawnedCards[i]) spawnedCards[i].gameObject.SetActive(true);
    }

    /// <summary>
    /// 생성된 카드들만 숨기기(Hand 오브젝트/씬 템플릿은 그대로)
    /// </summary>
    public void HideCards()
    {
        for (int i = 0; i < spawnedCards.Count; i++)
            if (spawnedCards[i]) spawnedCards[i].gameObject.SetActive(false);

        // 씬 템플릿은 항상 비활성 유지
        DeactivateSceneTemplates();
    }

    // CardUI → 클릭 콜백
    public void OnClickCard(int handIndex)
    {
        Debug.Log($"[BattleHandUI] 카드 클릭: index={handIndex}");
        // TODO: 이후 사용/공격 로직 연결
=======
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
>>>>>>> ba18b172bfd51ffca4970fd4549e0728dd6d1a79
>>>>>>> Stashed changes
    }
}
