// Assets/Script/Battle/Card_script/CardAnimeController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardAnimeController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HandUI playerHandUI;
    [SerializeField] private EnemyHandUI enemyHandUI;

    [Header("Motion")]
    [Tooltip("카드가 내려올 시작 오프셋(+Y에서 0으로)")]
    [SerializeField] private float startYOffset = 361f;
    [Tooltip("각 카드 1장의 이동/페이드 시간")]
    [SerializeField] private float perCardDuration = 0.25f;
    [Tooltip("카드 사이 시작 간격(스태거)")]
    [SerializeField] private float perCardStagger = 0.15f;

    [Header("Visual")]
    [Tooltip("애니메이션 중 알파 0→1로 페이드")]
    [SerializeField] private bool fadeAlpha = true;

    bool busy;

    void Reset()
    {
        if (!playerHandUI) playerHandUI = FindObjectOfType<HandUI>(true);
        if (!enemyHandUI) enemyHandUI = FindObjectOfType<EnemyHandUI>(true);
    }

    // 외부 API -------------------------------------------------------------

    /// <summary>플레이어 손패 초기 공개(1회) 애니메이션</summary>
    public Coroutine RevealInitialPlayerHand() => StartCoroutine(Co_RevealHand(playerHandUI));

    /// <summary>적 손패 초기 공개(1회) 애니메이션</summary>
    public Coroutine RevealInitialEnemyHand() => StartCoroutine(Co_RevealHand(enemyHandUI));

    // 내부 ---------------------------------------------------------------

    private IEnumerator Co_RevealHand(object handUI)
    {
        if (busy || handUI == null) yield break;
        busy = true;

        // 현재 스폰된 카드들의 RectTransform 스냅샷
        List<RectTransform> rects = null;
        if (handUI is HandUI p)
        {
            p.ShowCards();                         // 반드시 보이게
            rects = p.GetAllCardRects();           // (확장 메서드 아래 참고)
        }
        else if (handUI is EnemyHandUI e)
        {
            e.ShowAll();
            rects = e.GetAllCardRects();
        }

        if (rects == null || rects.Count == 0) { busy = false; yield break; }

        // 초기 상태 세팅: 각 카드 Y를 +startYOffset만큼 아래에 두고, 알파 0
        var baselines = new List<Vector2>(rects.Count);
        for (int i = 0; i < rects.Count; i++)
        {
            var rt = rects[i];
            baselines.Add(rt.anchoredPosition);
            rt.anchoredPosition = baselines[i] + new Vector2(0f, -startYOffset);

            if (fadeAlpha)
            {
                var cg = rt.GetComponent<CanvasGroup>();
                if (!cg) cg = rt.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                // 입력은 어차피 카드가 아니라 전체 HandUI에서 처리하므로 막을 필요 없음
            }
        }

        // 스태거를 두고 한 장씩 애니메이션 시작
        var running = new List<Coroutine>(rects.Count);
        for (int i = 0; i < rects.Count; i++)
        {
            running.Add(StartCoroutine(Co_TweenCard(rects[i], baselines[i])));
            if (perCardStagger > 0f) yield return new WaitForSeconds(perCardStagger);
        }

        // 모든 트윈 종료 대기(안전)
        foreach (var c in running) if (c != null) yield return c;

        busy = false;
    }

    private IEnumerator Co_TweenCard(RectTransform rt, Vector2 targetPos)
    {
        float t = 0f;
        var startPos = rt.anchoredPosition;
        CanvasGroup cg = fadeAlpha ? rt.GetComponent<CanvasGroup>() : null;

        while (t < perCardDuration)
        {
            t += Time.unscaledDeltaTime;  // UI 연출은 보통 unscaled
            float u = Mathf.Clamp01(t / perCardDuration);

            // easeOutCubic
            float e = 1f - Mathf.Pow(1f - u, 3f);

            rt.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, e);
            if (cg) cg.alpha = Mathf.LerpUnclamped(0f, 1f, e);

            yield return null;
        }

        rt.anchoredPosition = targetPos;
        if (cg) cg.alpha = 1f;
    }

    /// <summary>드로우 등으로 방금 추가된 카드들만 애니메이션</summary>
    public Coroutine RevealLastNCards(Faction side, int n)
    {
        if (n <= 0) return null;

        if (side == Faction.Player)
            return StartCoroutine(Co_RevealLastN_FromHandUI(playerHandUI, n));
        else
            return StartCoroutine(Co_RevealLastN_FromEnemyUI(enemyHandUI, n));
    }

    private IEnumerator Co_RevealLastN_FromHandUI(HandUI ui, int n)
    {
        if (ui == null) yield break;

        // HandUI가 RebuildFromHand로 프리팹을 만든 뒤여야 하므로 한 프레임 안전 대기
        yield return new WaitForEndOfFrame();

        var rectsAll = ui.GetAllCardRects();
        if (rectsAll == null || rectsAll.Count == 0) yield break;

        int start = Mathf.Max(0, rectsAll.Count - n);
        var subset = rectsAll.GetRange(start, rectsAll.Count - start);

        yield return StartCoroutine(Co_RevealSubset(subset));
    }

    private IEnumerator Co_RevealLastN_FromEnemyUI(EnemyHandUI ui, int n)
    {
        if (ui == null) yield break;

        yield return new WaitForEndOfFrame();

        var rectsAll = ui.GetAllCardRects();
        if (rectsAll == null || rectsAll.Count == 0) yield break;

        int start = Mathf.Max(0, rectsAll.Count - n);
        var subset = rectsAll.GetRange(start, rectsAll.Count - start);

        yield return StartCoroutine(Co_RevealSubset(subset));
    }

    private IEnumerator Co_RevealSubset(List<RectTransform> subset)
    {
        if (subset == null || subset.Count == 0) yield break;

        // 초기 상태: 아래에서 시작 + (옵션) 알파 0
        for (int i = 0; i < subset.Count; i++)
        {
            var rt = subset[i];
            rt.anchoredPosition += new Vector2(0f, -startYOffset);

            if (fadeAlpha)
            {
                var cg = rt.GetComponent<CanvasGroup>();
                if (!cg) cg = rt.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
            }
        }

        // 한 장씩 스태거로 트윈
        var coros = new List<Coroutine>(subset.Count);
        for (int i = 0; i < subset.Count; i++)
        {
            var target = subset[i].anchoredPosition + new Vector2(0f, startYOffset);
            coros.Add(StartCoroutine(Co_TweenCard(subset[i], target)));
            if (perCardStagger > 0f) yield return new WaitForSeconds(perCardStagger);
        }
        foreach (var c in coros) if (c != null) yield return c;
    }
}
