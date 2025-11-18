using UnityEngine;
using UnityEngine.SceneManagement;


public enum TurnState { PlayerTurn, EnemyTurn }


public class TurnManager : MonoBehaviour
{

    [Header("Move_Tutorial Intro")]
    [SerializeField] private bool moveTutorialIntro = true;     // 튜토리얼 씬에서만 켜두기
    [SerializeField, TextArea] private string introMsg1 = "넌 여기서 사라져야해...";
    [SerializeField, TextArea] private string introMsg2 = "일단.... 무서워..... 피해야해...!!";
    [SerializeField] private float introMsg1Seconds = 1.2f;     // 첫 대사 유지시간
    [SerializeField] private float introMsg2Seconds = 1.2f;     // 두 번째 대사 유지시간
    [SerializeField] private bool introRequireKey = false;       // true면 E키로 넘김
    [SerializeField] private KeyCode introKey = KeyCode.E;

    private bool tutorialIntroPlayed = false;
    private static bool IsMoveTutorial()
    => SceneManager.GetActiveScene().name == "Move_Tutorial";
    public static TurnManager Instance;

    // --- Move_Tutorial 전용 게이트 ---
    [Header("Move_Tutorial Gate")]
    [SerializeField] private bool moveTutorialGate = true;   // 이 씬에서만 켜두기
    [SerializeField] private float postEnemyWait = 3f;       // 적 턴 끝난 뒤 기본 대기
    [SerializeField] private KeyCode continueKey = KeyCode.E;
    [TextArea][SerializeField] private string gateMsg1 = "이 공격들을 피한다고....?(E키눌러서 계속)";
    [TextArea][SerializeField] private string gateMsg2 = "역시 너는 이 세상에 있으면 안돼...";


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
        // ▶ Move_Tutorial일 때는 먼저 인트로를 처리한 뒤, 적 턴을 시작
        if (IsMoveTutorial() && moveTutorialIntro && !tutorialIntroPlayed)
        {
            StartCoroutine(Co_MoveTutorialIntroBoot());  // ⬅ 새 코루틴
            return;
        }

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

        Debug.Log("🔶 적 턴 종료");

        if (moveTutorialGate && IsMoveTutorial())
        {
            // 관전 유지: 입력 잠금
            if (menu) menu.EnableInput(false);
            yield return StartCoroutine(Co_MoveTutorialGate());
            yield break; // 게이트 코루틴 안에서 BeginPlayerTurn 호출
        }

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
    private System.Collections.IEnumerator Co_MoveTutorialGate()
    {
        // 3초 대기
        if (postEnemyWait > 0f)
            yield return new WaitForSeconds(postEnemyWait);

        // 1차 프롬프트
        if (desc) desc.ShowTemporaryExplanation(gateMsg1);

        // [1] 첫 입력(E키 Down) 기다림
        while (!Input.GetKeyDown(continueKey))
            yield return null;

        // 같은 입력을 연속 인식하지 않도록, 키가 올라갈 때까지 대기
        yield return null;
        while (Input.GetKey(continueKey))
            yield return null;

        // 2차 프롬프트
        if (desc) desc.ShowTemporaryExplanation(gateMsg2);

        // [2] 두 번째 입력(E키 Down) 기다림
        while (!Input.GetKeyDown(continueKey))
            yield return null;

        // 마지막으로 키가 올라갈 때까지 잠깐 대기(선택)
        yield return null;
        while (Input.GetKey(continueKey))
            yield return null;

        // 클린업 후 내 턴
        if (desc) desc.ClearTemporaryMessage();
        BeginPlayerTurn();
    }

    private System.Collections.IEnumerator Co_MoveTutorialIntroBoot()
    {
        // 입력/포커스에 의한 패널 덮어쓰기 방지
        if (menu) menu.EnableInput(false);
        if (handUI) handUI.HideCards();
        if (desc) { desc.SetEnemyTurn(true); desc.SetPlayerDiscardMode(false); }

        tutorialIntroPlayed = true;

        // 1) 첫 문장
        if (desc) desc.ShowTemporaryExplanation(introMsg1);
        if (introRequireKey)
        {
            while (!Input.GetKeyDown(introKey)) yield return null;
            yield return null; while (Input.GetKey(introKey)) yield return null;
        }
        else if (introMsg1Seconds > 0f) yield return new WaitForSeconds(introMsg1Seconds);

        // 2) 둘째 문장
        if (desc) desc.ShowTemporaryExplanation(introMsg2);
        if (introRequireKey)
        {
            while (!Input.GetKeyDown(introKey)) yield return null;
            yield return null; while (Input.GetKey(introKey)) yield return null;
        }
        else if (introMsg2Seconds > 0f) yield return new WaitForSeconds(introMsg2Seconds);

        if (desc) desc.ClearTemporaryMessage();

        // 이제 실제 적 턴 시작
        BeginEnemyTurn();
    }


}
