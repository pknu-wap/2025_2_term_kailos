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

    bool busy;

    void Reset()
    {
        if (!hand) hand = FindObjectOfType<HandUI>(true);
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!costController) costController = FindObjectOfType<CostController>(true);
        if (!database) database = Resources.Load<CardDatabaseSO>("CardDatabase");
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

        // 1) SO 조회 (HandUI 스냅샷 기준)
        string id = hand.GetVisibleIdAt(handIndex);
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[Orchestrator] empty id");
            busy = false; yield break;
        }

        var so = database ? database.GetById(id) : null;
        if (!so)
        {
            Debug.LogWarning($"[Orchestrator] SO not found for id={id}");
            busy = false; yield break;
        }

        // 2) 코스트 체크/지불
        if (costController)
        {
            if (!costController.TryPay(Mathf.Max(0, so.cost)))
            {
                Debug.Log($"[Orchestrator] Not enough cost. need={so.cost}, cur={costController.Current}");
                busy = false; yield break;
            }
        }
        else
        {
            Debug.LogWarning("[Orchestrator] CostController missing — skipping cost check.");
        }

        // 3) 타입별 실행 (현재는 디버그/스텁)
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

        // 4) 패 소모(손패→덱 아래)
        var bdr = BattleDeckRuntime.Instance;
        if (bdr != null)
            bdr.UseCardToBottom(handIndex);

        // 5) 한 프레임 대기(리빌드 보장)
        yield return null;

        // 6) 선택 모드로 복귀(+안전하게 ShowCards)
        if (menu) menu.EnableInput(false);
        if (hand && hand.CardCount > 0)
        {
            hand.ShowCards();               // ✅ 혹시라도 꺼졌다면 켜기
            hand.EnterSelectMode();
            int nextIdx = Mathf.Clamp(handIndex, 0, hand.CardCount - 1);
            hand.SetSelectIndexPublic(nextIdx);
        }
        else
        {
            if (menu) menu.EnableInput(true);
        }

        busy = false;
    }
}
