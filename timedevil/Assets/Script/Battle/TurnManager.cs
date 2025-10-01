using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum TurnState { PlayerTurn, EnemyTurn }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public BattleHandUI handUI;
    public TurnState currentTurn;

    [Header("UI Buttons (플레이어 턴 활성/비활성)")]
    public Button cardBtn;
    public Button moveBtn;
    public Button itemBtn;
    public Button runBtn;

    [Header("Enemy Controller")]
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private BattleHandUI handUI;


    [Header("Delays")]
    [Tooltip("적 턴 시작 시 잠깐의 '고민시간' (초)")]
    public float enemyThinkDelay = 0.6f;

    /// <summary>이 턴에 카드를 이미 사용했는지(한 턴 1장 제한)</summary>
    public bool usedCardThisTurn { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        StartPlayerTurn();
    }

    // ---------------- Player Turn ----------------

    public void StartPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        SetButtons(true);

        // 손패 갱신 책임은 BattleHandUI 쪽으로 넘김
        if (handUI) handUI.OnPlayerTurnStart();

        Debug.Log("🔷 플레이어 턴 시작");
    }


    /// <summary>
    /// 외부에서 호출: 이 턴의 카드 사용권을 소모(한 턴 1장 제한)
    /// - 이미 사용했으면 false 반환
    /// - 아직이면 true 반환하면서 사용 처리
    /// </summary>
    public bool TryConsumeCardUseThisTurn()
    {
        if (usedCardThisTurn) return false;
        usedCardThisTurn = true;
        return true;
    }

    public void EndPlayerTurn()
    {
        // 엔드 페이즈: 손패가 3장 초과면 초과분을 덱 밑으로
        var bd = BattleDeckRuntime.Instance;
        if (bd != null)
        {
            while (bd.hand.Count > 3)
            {
                // 맨 오른쪽(마지막)부터 버린다고 가정
                bd.UseCardToBottom(bd.hand.Count - 1);
            }
        }

        SetButtons(false);
        if (handUI) handUI.Refresh();

        StartCoroutine(Co_EnemyTurn());
    }

    // ---------------- Enemy Turn ----------------

    IEnumerator Co_EnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("🔶 적 턴 시작");

        if (enemyThinkDelay > 0f)
            yield return new WaitForSeconds(enemyThinkDelay);

        if (enemyController != null)
        {
            // 적 행동 1회(이동 or 공격)
            yield return enemyController.ExecuteOneAction();
        }
        else
        {
            Debug.LogWarning("[TurnManager] EnemyController가 연결되어 있지 않습니다.");
        }

        Debug.Log("🔶 적 턴 종료");
        StartPlayerTurn();
    }

    // ---------------- Helpers ----------------

    void SetButtons(bool on)
    {
        if (cardBtn) cardBtn.interactable = on;
        if (moveBtn) moveBtn.interactable = on;
        if (itemBtn) itemBtn.interactable = on;
        if (runBtn) runBtn.interactable = on;
    }
}
