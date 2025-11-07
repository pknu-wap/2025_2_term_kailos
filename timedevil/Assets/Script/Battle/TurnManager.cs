using UnityEngine;

public enum TurnState { PlayerTurn, EnemyTurn }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [Header("Optional UI Controller")]
    [SerializeField] private BattleMenuController menu;

    [Header("Refs")]
    [SerializeField] private EnemyTurnController enemyTurnController;
    [SerializeField] private HandUI handUI;
    [SerializeField] private CostController cost;
    [SerializeField] private DescriptionPanelController desc;
    [SerializeField] private BattleDeckRuntime deck;

    [Header("Delays")]
    [SerializeField] private float enemyThinkDelay = 0.6f;
    [SerializeField] private EnemyHandUI enemyHandUI;
    [SerializeField] private EnemyDeckRuntime enemyDeck;
    [SerializeField] private ItemHandUI itemHand;
    [SerializeField] private float enemyDiscardRevealDelay = 3f;   // ✅ 추가: 적 버림 후 보여줄 시간(초)
    [SerializeField] private CardAnimeController cardAnime;

    private bool playerInitialRevealDone = false;
    private bool enemyInitialRevealDone = false;

    public bool IsPlayerDiscardPhase { get; private set; } = false;
    public TurnState currentTurn { get; private set; } = TurnState.PlayerTurn;

    private PlayerDataRuntime pdr;
    private EnemyRuntime enemyRt;
    private int playerSPD = 0;
    private int enemySPD = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!menu) menu = FindObjectOfType<BattleMenuController>(true);
        if (!enemyTurnController) enemyTurnController = FindObjectOfType<EnemyTurnController>(true);
        if (!handUI) handUI = FindObjectOfType<HandUI>(true);
        if (!cost) cost = FindObjectOfType<CostController>(true);
        if (!desc) desc = FindObjectOfType<DescriptionPanelController>(true);
        if (!deck) deck = BattleDeckRuntime.Instance ?? FindObjectOfType<BattleDeckRuntime>(true);
        if (!enemyHandUI) enemyHandUI = FindObjectOfType<EnemyHandUI>(true);
        if (!enemyDeck) enemyDeck = EnemyDeckRuntime.Instance ?? FindObjectOfType<EnemyDeckRuntime>(true);
        if (!itemHand) itemHand = FindObjectOfType<ItemHandUI>(true);
    }

    void Start()
    {
        pdr = FindObjectOfType<PlayerDataRuntime>(true);
        enemyRt = EnemyRuntime.Instance ?? FindObjectOfType<EnemyRuntime>(true);

        ResolvePlayerData();
        ResolveEnemyData();
        DecideFirstTurn();
    }

    void ResolvePlayerData()
    {
        if (pdr && pdr.Data != null) playerSPD = Mathf.Max(0, pdr.Data.speed);
        else { playerSPD = 0; Debug.LogWarning("[TurnManager] PlayerDataRuntime/Data 없음 → SPD=0"); }
    }

    void ResolveEnemyData()
    {
        if (enemyRt != null) enemySPD = Mathf.Max(0, enemyRt.speed);
        else { enemySPD = 0; Debug.LogWarning("[TurnManager] EnemyRuntime 없음 → SPD=0"); }
    }

    void DecideFirstTurn()
    {
        Debug.Log($"[TurnManager] SPD Compare => Player:{playerSPD} vs Enemy:{enemySPD}");
        if (enemySPD > playerSPD) BeginEnemyTurn();
        else BeginPlayerTurn();
    }

    public void BeginPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        IsPlayerDiscardPhase = false;


        if (cost) cost.ResetTurn();
        if (deck) deck.DrawOneIfNeeded();

        if (handUI) handUI.ShowCards();
        if (menu) menu.EnableInput(true);
        if (desc) { desc.SetEnemyTurn(false); desc.SetPlayerDiscardMode(false); } // 🔸

        if (enemyHandUI) enemyHandUI.HideAll();
        if (itemHand) itemHand.SetEnemyTurn(false);

        // ✅ 플레이어 초기 손패 연출 (한 번만, 프레임 끝에)
        if (!playerInitialRevealDone && cardAnime != null)
        {
            playerInitialRevealDone = true;
            StartCoroutine(Co_RevealPlayerInitialAfterFrame());
        }

        Debug.Log("🔷 플레이어 턴 시작");
    }

    public void BeginEnemyTurn()
    {
        if (itemHand) itemHand.SetEnemyTurn(true);

        currentTurn = TurnState.EnemyTurn;
        IsPlayerDiscardPhase = false;

        if (cost) cost.ResetTurn();


        if (menu) menu.EnableInput(false);
        if (handUI) handUI.HideCards();
        if (desc) { desc.SetEnemyTurn(true); desc.SetPlayerDiscardMode(false); } // 🔸

        if (enemyHandUI) { enemyHandUI.gameObject.SetActive(true); enemyHandUI.RebuildFromHand(); }

        // ✅ 적 초기 손패 연출 (한 번만, 프레임 끝에)
        if (!enemyInitialRevealDone && cardAnime != null)
        {
            enemyInitialRevealDone = true;
            StartCoroutine(Co_RevealEnemyInitialAfterFrame());
        }

        Debug.Log("🔶 적 턴 시작");
        StartCoroutine(Co_RunEnemyTurnThenBack());
    }

    System.Collections.IEnumerator Co_RunEnemyTurnThenBack()
    {
        if (enemyTurnController)
            yield return enemyTurnController.RunTurn();

        // ✅ 적 손패 초과 자동 버림(연출 → 데이터 이동 → 리빌드)
        if (enemyDeck != null && cardAnime != null)
        {
            int over = enemyDeck.OverCapCount;
            if (over > 0)
            {
                // 1) 역연출
                yield return cardAnime.DiscardLastNCards(
                    Faction.Enemy,
                    n: over,
                    fromRight: true,
                    afterAnimDataOp: () => enemyDeck.DiscardExcessToBottom(fromRight: true) // 2) 실제 이동
                );

                // 3) 보여주는 시간
                if (enemyDiscardRevealDelay > 0f)
                    yield return new WaitForSeconds(enemyDiscardRevealDelay);
            }
        }
        else
        {
            // 연출 컨트롤러 없을 때는 기존 로직 유지
            int dumped = 0;
            if (enemyDeck != null)
            {
                dumped = enemyDeck.DiscardExcessToBottom(fromRight: true);
                if (dumped > 0 && enemyHandUI) enemyHandUI.RebuildFromHand();
                if (dumped > 0 && enemyDiscardRevealDelay > 0f)
                    yield return new WaitForSeconds(enemyDiscardRevealDelay);
            }
        }

        Debug.Log("🔶 적 턴 종료 → 플레이어 턴");
        BeginPlayerTurn();
    }

    // EndController → 여기로 호출
    public void OnPlayerPressedEnd()
    {
        if (currentTurn != TurnState.PlayerTurn) return;

        // 초과 없음 → 바로 턴 종료
        if (deck == null || deck.OverCapCount <= 0)
        {
            OnPlayerActionCommitted();
            return;
        }

        // ✅ 강제 버림 페이즈 진입
        IsPlayerDiscardPhase = true;

        if (menu) menu.EnableInput(false);     // 메뉴 입력 잠금
        if (handUI)                            // 손패 선택 모드로 진입
        {
            handUI.ShowCards();
            handUI.EnterSelectMode();
        }

        // 안내 문구 고정

        if (desc)
        {
            desc.SetPlayerDiscardMode(true); // 🔸 여기 추가
            desc.ShowTemporaryExplanation($"손패가 {deck.MaxHandSize}장을 초과했습니다. 버릴 카드를 선택하세요. (남은 초과: {deck.OverCapCount})");
        }
        Debug.Log($"[TurnManager] DiscardPhase 시작 — 초과 {deck.OverCapCount}장");
    }

    // HandSelectController가 한 장 버릴 때마다 호출
    public void OnPlayerDiscardOne(int remainingOver)
    {
        if (!IsPlayerDiscardPhase) return;

        if (remainingOver > 0)
        {
            if (desc)
                desc.ShowTemporaryExplanation($"버릴 카드를 계속 선택하세요. (남은 초과: {remainingOver})");
            return;
        }

        // 버림 완료
        IsPlayerDiscardPhase = false;
        if (desc)
        {
            desc.ClearTemporaryMessage();
            desc.SetPlayerDiscardMode(false); // 🔸 여기 추가
        }

        // 선택모드 종료하고 실제 턴 종료로 진행
        if (handUI) handUI.ExitSelectMode();
        OnPlayerActionCommitted();
    }

    // 기존 End 확정 시 호출되던 함수 (변경 없음)
    public void OnPlayerActionCommitted()
    {
        if (currentTurn != TurnState.PlayerTurn) return;
        Debug.Log("[TurnManager] Player action committed → EnemyTurn");
        BeginEnemyTurn();
    }

    // ---- 새로 추가한 코루틴 2개 ----
    private System.Collections.IEnumerator Co_RevealPlayerInitialAfterFrame()
    {
        // HandUI가 카드 프리팹들을 배치할 시간을 준다
        yield return new WaitForEndOfFrame();
        yield return null; // 여유 프레임 하나 더 (UI 레이아웃 안정)
        if (cardAnime != null) cardAnime.RevealInitialPlayerHand();
    }

    private System.Collections.IEnumerator Co_RevealEnemyInitialAfterFrame()
    {
        // EnemyHandUI가 카드들을 만든 뒤에 연출 시작
        yield return new WaitForEndOfFrame();
        yield return null;
        if (cardAnime != null) cardAnime.RevealInitialEnemyHand();
    }
}
