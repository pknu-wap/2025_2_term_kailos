using UnityEngine;
using UnityEngine.SceneManagement;

public enum TurnState { PlayerTurn, EnemyTurn }

public class TurnManager : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Move_Tutorial 인트로: 영구 저장 + 세션 캐시
    // ─────────────────────────────────────────
    private const string PREF_KEY_MOVE_TUTORIAL_SEEN = "Move_Tutorial_Seen";
    private static bool s_MoveTutorialSeenThisSession = false;

    [Header("Move_Tutorial Intro")]
    [SerializeField] private bool moveTutorialIntro = true;     // 튜토리얼 씬에서만 켜두기
    [SerializeField] private bool forceIntroThisRun = false;    // ⬅ 테스트/디버그용: 이 실행에서 강제로 한 번 보이게
    [SerializeField, TextArea] private string introMsg1 = "넌 여기서 사라져야해...";
    [SerializeField, TextArea] private string introMsg2 = "일단.... 무서워..... 피해야해...!!";
    [SerializeField] private float introMsg1Seconds = 1.2f;
    [SerializeField] private float introMsg2Seconds = 1.2f;
    [SerializeField] private bool introRequireKey = false;
    [SerializeField] private KeyCode introKey = KeyCode.E;

    private bool tutorialIntroPlayed = false;
    private static bool IsMoveTutorial() => SceneManager.GetActiveScene().name == "Move_Tutorial";
    public static TurnManager Instance;

    // --- Move_Tutorial 전용 게이트 ---
    [Header("Move_Tutorial Gate")]
    [SerializeField] private bool moveTutorialGate = true;
    [SerializeField] private float postEnemyWait = 3f;
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
    [SerializeField] private float enemyDiscardRevealDelay = 3f;
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

        // 저장된 적 있으면 세션 캐시 올림
        if (PlayerPrefs.GetInt(PREF_KEY_MOVE_TUTORIAL_SEEN, 0) == 1)
            s_MoveTutorialSeenThisSession = true;
    }

    void Start()
    {
        pdr = FindObjectOfType<PlayerDataRuntime>(true);
        enemyRt = EnemyRuntime.Instance ?? FindObjectOfType<EnemyRuntime>(true);

        ResolvePlayerData();
        ResolveEnemyData();

        // ▶ Move_Tutorial이면 인트로 우선 검사
        if (IsMoveTutorial() && moveTutorialIntro && ShouldPlayIntroNow())
        {
            Debug.Log("[TurnManager] Move_Tutorial intro start");
            StartCoroutine(Co_MoveTutorialIntroBoot());
            return; // ⬅ 인트로가 턴 진행을 막도록 즉시 반환
        }

        // 그 외: 정상 시작
        DecideFirstTurn();
    }

    // 인트로 재생 여부 판단(강제 옵션 반영)
    private bool ShouldPlayIntroNow()
    {
        if (forceIntroThisRun) return true; // 테스트용 강제 재생
        if (tutorialIntroPlayed) return false; // 이미 재생 시작했으면 X

        bool seenGlobally = (PlayerPrefs.GetInt(PREF_KEY_MOVE_TUTORIAL_SEEN, 0) == 1);
        bool seenThisSession = s_MoveTutorialSeenThisSession;

        return !(seenGlobally || seenThisSession);
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
        if (desc) { desc.SetEnemyTurn(false); desc.SetPlayerDiscardMode(false); }

        if (enemyHandUI) enemyHandUI.HideAll();
        if (itemHand) itemHand.SetEnemyTurn(false);

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
        if (desc) { desc.SetEnemyTurn(true); desc.SetPlayerDiscardMode(false); }

        if (enemyHandUI) { enemyHandUI.gameObject.SetActive(true); enemyHandUI.RebuildFromHand(); }

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

        // 적 손패 초과 자동 버림(연출 → 데이터 이동 → 리빌드)
        if (enemyDeck != null && cardAnime != null)
        {
            int over = enemyDeck.OverCapCount;
            if (over > 0)
            {
                yield return cardAnime.DiscardLastNCards(
                    Faction.Enemy,
                    n: over,
                    fromRight: true,
                    afterAnimDataOp: () => enemyDeck.DiscardExcessToBottom(fromRight: true)
                );

                if (enemyDiscardRevealDelay > 0f)
                    yield return new WaitForSeconds(enemyDiscardRevealDelay);
            }
        }
        else
        {
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
            if (menu) menu.EnableInput(false);
            yield return StartCoroutine(Co_MoveTutorialGate());
            yield break; // 게이트 코루틴 안에서 BeginPlayerTurn 호출
        }

        BeginPlayerTurn();
    }

    public void OnPlayerPressedEnd()
    {
        if (currentTurn != TurnState.PlayerTurn) return;

        if (deck == null || deck.OverCapCount <= 0)
        {
            OnPlayerActionCommitted();
            return;
        }

        IsPlayerDiscardPhase = true;

        if (menu) menu.EnableInput(false);
        if (handUI)
        {
            handUI.ShowCards();
            handUI.EnterSelectMode();
        }

        if (desc)
        {
            desc.SetPlayerDiscardMode(true);
            desc.ShowTemporaryExplanation($"손패가 {deck.MaxHandSize}장을 초과했습니다. 버릴 카드를 선택하세요. (남은 초과: {deck.OverCapCount})");
        }
        Debug.Log($"[TurnManager] DiscardPhase 시작 — 초과 {deck.OverCapCount}장");
    }

    public void OnPlayerDiscardOne(int remainingOver)
    {
        if (!IsPlayerDiscardPhase) return;

        if (remainingOver > 0)
        {
            if (desc)
                desc.ShowTemporaryExplanation($"버릴 카드를 계속 선택하세요. (남은 초과: {remainingOver})");
            return;
        }

        IsPlayerDiscardPhase = false;
        if (desc)
        {
            desc.ClearTemporaryMessage();
            desc.SetPlayerDiscardMode(false);
        }

        if (handUI) handUI.ExitSelectMode();
        OnPlayerActionCommitted();
    }

    public void OnPlayerActionCommitted()
    {
        if (currentTurn != TurnState.PlayerTurn) return;
        Debug.Log("[TurnManager] Player action committed → EnemyTurn");
        BeginEnemyTurn();
    }

    private System.Collections.IEnumerator Co_RevealPlayerInitialAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        yield return null;
        if (cardAnime != null) cardAnime.RevealInitialPlayerHand();
    }

    private System.Collections.IEnumerator Co_RevealEnemyInitialAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        yield return null;
        if (cardAnime != null) cardAnime.RevealInitialEnemyHand();
    }

    private System.Collections.IEnumerator Co_MoveTutorialGate()
    {
        if (postEnemyWait > 0f)
            yield return new WaitForSeconds(postEnemyWait);

        if (desc) desc.ShowTemporaryExplanation(gateMsg1);
        while (!Input.GetKeyDown(continueKey)) yield return null;
        yield return null; while (Input.GetKey(continueKey)) yield return null;

        if (desc) desc.ShowTemporaryExplanation(gateMsg2);
        while (!Input.GetKeyDown(continueKey)) yield return null;
        yield return null; while (Input.GetKey(continueKey)) yield return null;

        if (desc) desc.ClearTemporaryMessage();
        BeginPlayerTurn();
    }

    private System.Collections.IEnumerator Co_MoveTutorialIntroBoot()
    {
        // 인트로 중에는 어떤 턴도 진행되지 않도록 UI/입력 잠금
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

        // ▶ 인트로를 실제로 보여주었으므로 플래그 저장
        s_MoveTutorialSeenThisSession = true;
        PlayerPrefs.SetInt(PREF_KEY_MOVE_TUTORIAL_SEEN, 1);
        PlayerPrefs.Save();

        // 이제 실제 적 턴 시작
        BeginEnemyTurn();
    }

#if UNITY_EDITOR
    // F12: 인트로 재생 플래그 초기화
    void Update()
    {
        if (IsMoveTutorial() && Input.GetKeyDown(KeyCode.F12))
        {
            PlayerPrefs.DeleteKey(PREF_KEY_MOVE_TUTORIAL_SEEN);
            s_MoveTutorialSeenThisSession = false;
            Debug.LogWarning("[TurnManager] Move_Tutorial 인트로 플래그 초기화됨 (에디터 F12)");
        }
    }
#endif
}
