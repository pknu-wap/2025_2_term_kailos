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

    [Header("Delays")]
    [Tooltip("적 턴 시작 시 잠깐의 '고민시간' (초)")]
    public float enemyThinkDelay = 0.6f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        SetButtons(true);
        Debug.Log("🔷 플레이어 턴 시작");
        // (다음 단계 확장 시: 손패 보충/한 턴 1장 리셋 등 여기서 처리)
    }

    public void EndPlayerTurn()
    {
        SetButtons(false);
        StartCoroutine(Co_EnemyTurn());
    }

    IEnumerator Co_EnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("🔶 적 턴 시작");

        if (enemyThinkDelay > 0f)
            yield return new WaitForSeconds(enemyThinkDelay);

        if (enemyController != null)
            yield return enemyController.ExecuteOneAction();
        else
            Debug.LogWarning("[TurnManager] EnemyController가 연결되어 있지 않습니다.");

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
}
