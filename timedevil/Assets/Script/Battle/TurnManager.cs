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

        if (handUI == null) handUI = FindObjectOfType<BattleHandUI>();

        if (cardBtn != null)
        {
            cardBtn.onClick.RemoveListener(OnPressCardButton);
            cardBtn.onClick.AddListener(OnPressCardButton);
        }
    }

    void Start()
    {
        // ❌ 불필요 → 초기 NRE 원인
        // if (handUI) handUI.SetVisible(false, "TurnManager.Start");

        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        SetButtons(true);

        if (handUI)
        {
            handUI.OnPlayerTurnStart();
            handUI.SetVisible(false, "StartPlayerTurn");
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
            handUI.SetVisible(false, "EndPlayerTurn");
        }

        SetButtons(false);
        StartCoroutine(Co_EnemyTurn());
    }

    IEnumerator Co_EnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("🔶 적 턴 시작");

        if (handUI) handUI.SetVisible(false, "EnemyTurn.Start");

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

    public void OnPressCardButton()
    {
        Debug.Log("[TurnManager] Card 버튼 눌림!");
        if (currentTurn != TurnState.PlayerTurn || handUI == null) return;

        Debug.Log("[TurnManager] Card 버튼 클릭 → HandUI.OpenAndRefresh()");
        handUI.OpenAndRefresh();
    }

    public void OnCardPanelClosed()
    {
        if (handUI) handUI.Close();
        SetButtons(true);
    }
}
