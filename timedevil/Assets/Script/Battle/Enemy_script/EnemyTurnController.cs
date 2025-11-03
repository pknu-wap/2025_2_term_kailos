using System.Collections;
using UnityEngine;

public class EnemyTurnController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HandUI handUI;                 // 적 턴 동안 관전 모드로 바인딩
    [SerializeField] private EnemyDeckRuntime enemyDeck;    // 적 손패/덱
    [SerializeField] private CardDatabaseSO database;       // id -> SO
    [SerializeField] private CostController costController; // 턴 자원(적 턴 시작시 10으로 리셋)
    [SerializeField] private ShowCardController showCard;   // 선택 카드 3초 노출
    [SerializeField] private BattleMenuController menu;     // 플레이어 입력 OFF/ON

    [Header("Rules")]
    [SerializeField] private int enemyTurnCost = 10;
    [SerializeField] private float showSeconds = 3f;

    private bool _running;

    void Reset()
    {
        if (!handUI) handUI = FindObjectOfType<HandUI>(true);
        if (!enemyDeck) enemyDeck = EnemyDeckRuntime.Instance ?? FindObjectOfType<EnemyDeckRuntime>(true);
        if (!database) database = Resources.Load<CardDatabaseSO>("CardDatabase");
        if (!costController) costController = FindObjectOfType<CostController>(true);
        if (!showCard) showCard = FindObjectOfType<ShowCardController>(true);
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
    }

    public IEnumerator RunTurn()
    {
        if (_running) yield break;
        _running = true;

        // 1) 준비
        if (menu) menu.EnableInput(false);
        if (costController) costController.ResetTo(enemyTurnCost);
        if (enemyDeck) enemyDeck.DrawOneIfNeeded();

        if (handUI) { handUI.BindToEnemy(); handUI.RebuildFromHand(); }

        yield return null; // 한 프레임 정리

        // 2) 카드 사용 루프
        bool anyPlayedThisLoop;
        while (true)
        {
            anyPlayedThisLoop = false;
            var ids = handUI ? handUI.VisibleHandIds : enemyDeck?.GetHandIds();
            if (ids == null || ids.Count == 0) break;

            for (int i = 0; i < ids.Count; i++)
            {
                string id = ids[i];
                var so = database ? database.GetById(id) : null;
                if (!so) continue;

                int need = Mathf.Max(0, so.cost);
                if (costController && costController.Current < need)
                    continue;

                // 선택 표시(관전)
                if (handUI) handUI.SetSelectIndexPublic(i);

                // Hand 칸 이미지는 즉시 숨김 + ShowCard 3초 노출
                if (handUI) handUI.SetCardEnabled(i, false);
                if (showCard) yield return showCard.ShowForSecondsById(id, showSeconds);

                // === 프리뷰 직후 안전 복구 ===
                if (handUI) handUI.EnableAllCardImages();

                // 카드 효과(지금은 로그만)
                Debug.Log($"[EnemyTurn] Use {id} (cost={so.cost})");

                // 코스트 지불
                if (costController) costController.TryPay(need);

                // 사용한 카드는 덱 아래로
                if (enemyDeck) enemyDeck.UseCardToBottom(i);

                // HandUI가 리빌드되도록 한 프레임 양보
                yield return null;

                // 한 장 썼으면 다시 왼쪽부터 스캔
                anyPlayedThisLoop = true;
                break;
            }

            if (!anyPlayedThisLoop) break; // 더 못 내면 종료
        }

        // 3) 턴 종료 전 최종 동기화
        if (handUI)
        {
            handUI.EnableAllCardImages();
            handUI.RebuildFromHand();
        }

        _running = false;
    }
}
