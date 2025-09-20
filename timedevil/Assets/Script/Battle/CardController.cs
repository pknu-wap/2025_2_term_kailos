using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 카드 사용 컨트롤러 (JSON 덱 기반)
/// - 덱을 CardSaveStore / CardStateRuntime에서 로드
/// - 첫 번째 카드 1장을 UI에 표시하고, 클릭 시 패턴 발동
/// - 발동 후 턴 종료 (연출 시간만큼 대기 후 종료)
/// </summary>
public class CardController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button cardOpenButton;     // 카드 열기 버튼
    [SerializeField] private GameObject cardGroup;      // 카드 패널(그룹)
    [SerializeField] private Image cardImage;           // 카드 아이콘
    [SerializeField] private Button cardImageButton;    // 카드 사용 버튼(이미지 클릭)

    [Header("Attack")]
    [SerializeField] private AttackController attackController;

    [Header("Resources")]
    [SerializeField] private string resourcesFolder = "my_asset"; // Resources/my_asset/<CardId>

    // 내부 상태
    private string _currentCardId;    // UI에 표시 중인 카드(덱의 첫 장)
    private CardSaveData _deckData;   // 현재 세션 덱 데이터(파일에서 로드)

    void Awake()
    {
        // 버튼 리스너 초기화
        if (cardOpenButton)
        {
            cardOpenButton.onClick.RemoveAllListeners();
            cardOpenButton.onClick.AddListener(OpenCardUI);
        }
        if (cardImageButton)
        {
            cardImageButton.onClick.RemoveAllListeners();
            cardImageButton.onClick.AddListener(UseCurrentCard);
        }

        // 시작 시 패널은 닫아둠
        if (cardGroup) cardGroup.SetActive(false);
    }

    void Start()
    {
        var cache = FindObjectOfType<CardStateRuntime>();
        _deckData = cache != null ? cache.Data : CardSaveStore.Load();

        // 덱이 비었으면 UI/데이터 초기화
        if (_deckData == null || _deckData.deck == null || _deckData.deck.Count == 0)
        {
            Debug.Log("[CardController] 덱이 비어있습니다. 카드 UI를 끕니다.");
            _currentCardId = null;  // ✅ 카드 ID 초기화
            if (cardOpenButton) cardOpenButton.interactable = false;
            if (cardGroup) cardGroup.SetActive(false);
            if (cardImage) cardImage.sprite = null;  // ✅ 이미지 제거
            return;
        }

        // 첫 장을 현재 카드로 세팅
        _currentCardId = _deckData.deck[0];
        RefreshCardIcon();
    }


    // ----- UI 핸들러 -----

    void OpenCardUI()
    {
        if (string.IsNullOrEmpty(_currentCardId))
        {
            Debug.LogWarning("[CardController] 현재 사용할 수 있는 카드가 없습니다.");
            return;
        }

        if (cardGroup) cardGroup.SetActive(true);
    }

    void CloseCardUI()
    {
        if (cardGroup) cardGroup.SetActive(false);
    }

    // ----- 동작 -----

    void RefreshCardIcon()
    {
        if (!cardImage) return;

        // 카드 아이콘 스프라이트 로드 (Resources/my_asset/<CardId>)
        var sprite = Resources.Load<Sprite>($"{resourcesFolder}/{_currentCardId}");
        if (!sprite)
        {
            Debug.LogWarning($"[CardController] 스프라이트를 찾을 수 없음: Resources/{resourcesFolder}/{_currentCardId}");
            cardImage.sprite = null;
            return;
        }
        cardImage.sprite = sprite;
    }

    /// <summary>
    /// 현재 카드(_currentCardId)를 사용하여 패턴 발동
    /// </summary>
    void UseCurrentCard()
    {
        if (string.IsNullOrEmpty(_currentCardId))
        {
            Debug.LogWarning("[CardController] 사용할 카드가 없습니다.");
            CloseCardUI();
            return;
        }
        if (attackController == null)
        {
            Debug.LogWarning("[CardController] attackController 참조가 없습니다.");
            CloseCardUI();
            return;
        }

        // ICardPattern 구현 타입을 이름으로 찾아 인스턴스화 (예: "Card1")
        var t = FindTypeByName(_currentCardId);
        if (t == null)
        {
            Debug.LogWarning($"[CardController] 카드 타입을 찾을 수 없음: '{_currentCardId}'");
            CloseCardUI();
            return;
        }

        // 임시 GO에 부착하여 데이터 읽기
        var go = new GameObject($"_PlayerCard_{_currentCardId}");
        try
        {
            var comp = go.AddComponent(t) as ICardPattern;
            if (comp == null)
            {
                Debug.LogWarning($"[CardController] 타입은 찾았으나 ICardPattern이 아님: '{_currentCardId}'");
                CloseCardUI();
                return;
            }

            // 적 패널(오른쪽)에 표시
            var timings = comp.Timings ?? new float[16];
            attackController.ShowPattern(comp.Pattern16, timings, AttackController.Panel.Enemy);

            // 연출 총 시간 후 턴 종료
            float total = attackController.GetSequenceDuration(timings);
            Invoke(nameof(EndPlayerTurn), total);
        }
        finally
        {
            Destroy(go);
        }

        // 카드 패널 닫기
        CloseCardUI();

        // 필요 시: 사용한 카드를 덱에서 제거/회전시키고 저장하고 싶다면 여기에 로직 추가 가능
        // (지금은 단순히 "첫 장을 항상 사용"하는 형태 유지)
    }

    void EndPlayerTurn()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.EndPlayerTurn();
    }

    // ----- 유틸 -----

    static Type FindTypeByName(string typeName)
    {
        var asm = typeof(CardController).Assembly;
        return asm.GetTypes().FirstOrDefault(t => t.Name == typeName && typeof(MonoBehaviour).IsAssignableFrom(t));
    }
}
