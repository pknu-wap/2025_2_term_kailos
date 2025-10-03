using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum TurnState { PlayerTurn, EnemyTurn }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public TurnState currentTurn;

    [Header("UI Buttons (플레이어 턴 활성/비활성)")]
    public Button cardBtn;
    public Button moveBtn;
    public Button itemBtn;
    public Button runBtn;

    [Header("Enemy Controller")]
    [SerializeField] private EnemyController enemyController;

    [Header("Hand UI")]
    [SerializeField] private BattleHandUI handUI;

    [Header("Delays")]
    public float enemyThinkDelay = 0.6f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 🔧 참조 보강: 씬에서 안 채워져 있어도 자동 주입
        if (handUI == null) handUI = FindObjectOfType<BattleHandUI>();

        // 🔧 버튼-이벤트 자동 연결(중복 방지)
        if (cardBtn != null)
        {
            cardBtn.onClick.RemoveListener(OnPressCardButton);
            cardBtn.onClick.AddListener(OnPressCardButton);
        }
    }

    void Start()
    {
        if (handUI) handUI.SetVisible(false);
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        SetButtons(true);

        if (handUI)
        {
            handUI.OnPlayerTurnStart();
            handUI.SetVisible(false);
        }

        Debug.Log("🔷 플레이어 턴 시작");
    }

    public void EndPlayerTurn()
    {
        var bd = BattleDeckRuntime.Instance;
        if (bd != null)
        {
            while (bd.hand.Count > 3)
                bd.UseCardToBottom(bd.hand.Count - 1);
        }

        if (handUI)
        {
            handUI.Refresh();
            handUI.SetVisible(false);
        }

        SetButtons(false);
        StartCoroutine(Co_EnemyTurn());
    }

    IEnumerator Co_EnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("🔶 적 턴 시작");

        if (handUI) handUI.SetVisible(false);

        if (enemyThinkDelay > 0f) yield return new WaitForSeconds(enemyThinkDelay);
        if (enemyController != null) yield return enemyController.ExecuteOneAction();
        else Debug.LogWarning("[TurnManager] EnemyController 미연결");

        Debug.Log("🔶 적 턴 종료");
        StartPlayerTurn();
    }

    void SetButtons(bool on)
    {
        if (cardBtn) cardBtn.interactable = on;
        if (moveBtn) moveBtn.interactable = on;
        if (itemBtn) itemBtn.interactable = on;
        if (runBtn) runBtn.interactable = on;
    }

    // ✅ Card 버튼이 눌리면 반드시 여기로 들어옴
    public void OnPressCardButton()
    {
        Debug.Log("[TurnManager] Card 버튼 눌림!");
        if (currentTurn != TurnState.PlayerTurn || handUI == null) return;

        Debug.Log("[TurnManager] Card 버튼 클릭 → HandUI.OpenAndRefresh()");
        handUI.OpenAndRefresh();   // 내부에서 CanvasGroup(α/Interact/Blocks) 켬
        SetButtons(false);         // (선택) 패널 열린 동안 다른 버튼 비활성
    }

    public void OnCardPanelClosed()
    {
        if (handUI) handUI.Close();
        SetButtons(true);
    }
}
