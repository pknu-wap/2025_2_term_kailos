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

    [Header("Controllers")]
    [SerializeField] private AttackController attackController;
    [SerializeField] private SupportController supportController;
    [SerializeField] private DrawController drawController;
    [SerializeField] private MoveController moveController;

    [Header("ShowCard Preview")]
    [SerializeField] private ShowCardController showCard;
    [SerializeField] private float showSeconds = 3f;

    private bool busy;

    void Reset()
    {
        if (!hand) hand = FindObjectOfType<HandUI>(true);
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!costController) costController = FindObjectOfType<CostController>(true);
        if (!database) database = Resources.Load<CardDatabaseSO>("CardDatabase");
        if (!showCard) showCard = FindObjectOfType<ShowCardController>(true);
    }

    public void UseCurrentSelected()
    {
        if (busy || hand == null || !hand.IsInSelectMode) return;
        int idx = hand.CurrentSelectIndex;
        if (idx < 0 || idx >= hand.CardCount) return;
        StartCoroutine(CoUseAtIndex(idx));
    }

    private IEnumerator CoUseAtIndex(int handIndex)
    {
        busy = true;

        // A) 카드 정보 확보
        string id = hand.GetVisibleIdAt(handIndex);
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[Orchestrator] empty id");
            busy = false;
            yield break;
        }
        var so = database ? database.GetById(id) : null;
        if (!so)
        {
            Debug.LogWarning($"[Orchestrator] SO not found for id={id}");
            busy = false;
            yield break;
        }

        // B) 코스트 체크
        if (costController && !costController.TryPay(Mathf.Max(0, so.cost)))
        {
            Debug.Log($"[Orchestrator] Not enough cost. need={so.cost}, cur={costController.Current}");
            busy = false; // 선택모드 유지
            yield break;
        }

        // C) 즉시 손패에서 제거(덱 아래로 이동) → 화면에서 바로 사라짐
        var bdr = BattleDeckRuntime.Instance;
        if (bdr != null) bdr.UseCardToBottom(handIndex);

        // HandUI가 리빌드될 시간을 한 프레임 부여
        yield return null;

        // D) 3초 프리뷰 (ShowCard)
        if (showCard) yield return showCard.ShowForSecondsById(so.id, showSeconds);

        // === 프리뷰 직후 안전 복구 ===
        if (hand)
        {
            hand.EnableAllCardImages();
            hand.RebuildFromHand();
        }

        // E) 타입별 실행
        Faction self = Faction.Player;
        Faction foe = Faction.Enemy;
        switch (so.type)
        {
            case CardType.Attack:
                if (attackController) yield return attackController.Execute(so as AttackCardSO, self, foe);
                break;
            case CardType.Support:
                if (supportController) yield return supportController.Execute(so as SupportCardSO, self, foe);
                break;
            case CardType.Draw:
                if (drawController) yield return drawController.Execute(so as DrawCardSO, self, foe);
                break;
            case CardType.Move:
                if (moveController) yield return moveController.Execute(so as MoveCardSO, self, foe);
                break;
            default:
                Debug.Log($"[Orchestrator] UNKNOWN -> id={id}, cost={so.cost}");
                break;
        }

        // F) 카드 선택 모드로 복귀 (최종 안전 복구 포함)
        if (hand)
        {
            hand.EnableAllCardImages();
            hand.RebuildFromHand();
        }

        if (menu) menu.EnableInput(false); // 메뉴는 잠깐 비활성
        if (hand && hand.CardCount > 0)
        {
            hand.EnterSelectMode();
            int nextIdx = Mathf.Clamp(handIndex, 0, hand.CardCount - 1);
            hand.SetSelectIndexPublic(nextIdx);
        }
        else
        {
            if (menu) menu.EnableInput(true); // 손패 비었으면 메뉴 입력 복구
        }

        busy = false;
    }
}
