// Assets/Script/Battle/Card_script/CardUseOrchestrator.cs
using System.Collections;
using UnityEngine;

public enum Faction { Player, Enemy }

public class CardUseOrchestrator : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HandUI hand;
    [SerializeField] private BattleMenuController menu;
    [SerializeField] private CardDatabaseSO database;

    [Header("Preview")]
    [SerializeField] private ShowCardController showCard;  // ✅ 새 컨트롤러 참조
    [SerializeField] private float totalSeconds = 3f;      // 총 길이를 바꾸고 싶으면 아래 ShowCard에서 조절

    private bool busy;

    void Reset()
    {
        if (!hand) hand = FindObjectOfType<HandUI>(true);
        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!database) database = Resources.Load<CardDatabaseSO>("CardDatabase");
        if (!showCard) showCard = FindObjectOfType<ShowCardController>(true);
    }

    public void UseCurrentSelected()
    {
        if (busy || hand == null || !hand.IsInSelectMode) return;
        int idx = hand.CurrentSelectIndex;
        if (idx < 0 || idx >= hand.CardCount) return;
        StartCoroutine(CoPreviewOnly(idx));
    }

    /// <summary>
    /// 카드 사용 대신 “미리보기만” 수행:
    /// - Card Select 모드 → spectator(=선택모드 해제 + 메뉴 입력 차단)로 전환
    /// - ShowCard 페이드 재생 (자기 손패는 계속 보임/비활성화 안함)
    /// - 끝나면 다시 Card Select 모드 복귀, 같은 인덱스에 커서 복구
    /// </summary>
    private IEnumerator CoPreviewOnly(int handIndex)
    {
        busy = true;

        string id = hand.GetVisibleIdAt(handIndex);
        if (string.IsNullOrEmpty(id))
        {
            busy = false;
            yield break;
        }

        // spectator mod1: 선택모드만 잠시 해제, 메뉴 입력은 차단
        if (hand.IsInSelectMode) hand.ExitSelectMode();
        if (menu) menu.EnableInput(false); // spectator 동안 메뉴 키 입력 막기
        hand.ShowCards();                  // 카드들은 계속 보이게

        // ShowCard 실행 (코스트/덱 조작 없음)
        if (showCard != null)
            yield return showCard.PreviewById(id);
        else
            yield return null;

        // Card Select 모드 복귀
        hand.EnterSelectMode();
        hand.SetSelectIndexPublic(Mathf.Clamp(handIndex, 0, hand.CardCount - 1));
        // 선택모드일 땐 메뉴 입력은 계속 OFF(HandSelectController가 관리)
        if (menu) menu.EnableInput(false);

        busy = false;
    }
}
