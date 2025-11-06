using System.Collections;
using UnityEngine;

public enum Faction { Player, Enemy }

public class CardUseOrchestrator : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HandUI hand;
    [SerializeField] private BattleMenuController menu;
    [SerializeField] private CardDatabaseSO database;
    [SerializeField] private CostController costController;

    [Header("Preview")]
    [SerializeField] private ShowCardController showCard;
    [SerializeField] private float totalSeconds = 3f;   // 페이드 포함 총 시간

    [Header("UI Hooks")]
    [SerializeField] private DescriptionPanelController desc; // 👈 관전 모드 대사 표시용
    [SerializeField] private bool logDebug = false; // ← 옵션 로그

    // (효과 실행은 타이밍 안정화 후 다시 연결)
    [Header("Optional Effect Controllers (disabled for timing)")]
    [SerializeField] private AttackController attackController;
    [SerializeField] private SupportController supportController;
    [SerializeField] private DrawController drawController;
    [SerializeField] private MoveController moveController;


    private bool busy;

    void Awake()
    {
        // 💡 런타임에서도 안전하게 참조 보강
        if (!hand) hand = FindObjectOfType<HandUI>(true);
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!database) database = Resources.Load<CardDatabaseSO>("CardDatabase");
        if (!costController) costController = FindObjectOfType<CostController>(true);
        if (!showCard) showCard = FindObjectOfType<ShowCardController>(true);
        if (!desc) desc = FindObjectOfType<DescriptionPanelController>(true);

        if (logDebug && !desc)
            Debug.LogWarning("[Orchestrator] DescriptionPanelController not found. Explanation won't show.");
    }

    public void UseCurrentSelected()
    {
        if (busy || hand == null || !hand.IsInSelectMode) return;
        int idx = hand.CurrentSelectIndex;
        if (idx < 0 || idx >= hand.CardCount) return;
        StartCoroutine(Co_UseWithExactTiming(idx));
    }

    /// <summary>
    /// 정확한 타이밍:
    /// 1) 카드 선택 → 코스트 즉시 지불(가능 여부 확인 포함)
    /// 2) 카드 즉시 사라짐(덱 아래로 이동)
    /// 3) 관전모드(선택 해제 + 메뉴 입력 OFF) + 설명판에 explanation 고정
    /// 4) ShowCard 프리뷰(페이드 인/유지/아웃)
    /// 5) 설명판 임시문구 해제 → 카드 선택 모드 복귀
    /// </summary>
    private IEnumerator Co_UseWithExactTiming(int handIndex)
    {
        busy = true;

        // A. 카드 SO 확보
        string id = hand.GetVisibleIdAt(handIndex);
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[Orchestrator] empty id");
            busy = false; yield break;
        }
        var so = database ? database.GetById(id) : null;
        if (!so)
        {
            Debug.LogWarning($"[Orchestrator] SO not found: id={id}");
            busy = false; yield break;
        }

        // B. 코스트 즉시 지불(선확인 + 차감)
        int need = Mathf.Max(0, so.cost);
        if (costController && costController.Current < need)
        {
            Debug.Log($"[Orchestrator] Not enough cost. need={need}, cur={costController.Current}");
            busy = false; yield break;
        }
        if (costController && !costController.TryPay(need))
        {
            Debug.LogWarning($"[Orchestrator] TryPay failed unexpectedly. need={need}, cur={costController.Current}");
            busy = false; yield break;
        }

        // C. 카드 즉시 제거(덱 아래) → 화면에서 바로 사라짐
        var bdr = BattleDeckRuntime.Instance;
        if (bdr != null) bdr.UseCardToBottom(handIndex);
        yield return null;               // 데이터 반영 프레임 양보
        hand.RebuildFromHand();          // 안전하게 스냅샷/오브젝트 재구성

        // D. 관전 모드: 선택 해제 + 메뉴 입력 OFF (손패는 계속 보임)
        hand.ExitSelectMode();
        if (menu) menu.EnableInput(false);

        // 👉 설명판에 explanation 고정(비어있으면 display, 그마저 없으면 기본 폴백)
        if (desc)
        {
            string line =
                !string.IsNullOrEmpty(so.explanation) ? so.explanation :
                (!string.IsNullOrEmpty(so.display) ? so.display :
                (!string.IsNullOrEmpty(so.displayName) ? so.displayName : so.id));

            desc.ShowTemporaryExplanation(line);

            if (logDebug)
                Debug.Log($"[Orchestrator] Explanation shown: {line}");
        }

        // E. ShowCard 프리뷰 (다른 UI엔 손대지 않음)
        if (showCard) yield return showCard.PreviewById(so.id, totalSeconds);
        else yield return null;

        // 👉 설명판 임시문구 해제
        if (desc) desc.ClearTemporaryMessage();

        // F. 카드 선택 모드 복귀(오른쪽 끝 유지 대신, 방금 위치로 보정)
        if (hand.CardCount > 0)
        {
            hand.EnterSelectMode();
            int nextIdx = Mathf.Clamp(handIndex, 0, hand.CardCount - 1);
            hand.SetSelectIndexPublic(nextIdx);
            if (menu) menu.EnableInput(false); // 선택 모드 유지 규칙
        }
        else
        {
            if (menu) menu.EnableInput(true); // 손패 비었으면 메뉴만 ON
        }

        busy = false;
    }


}
