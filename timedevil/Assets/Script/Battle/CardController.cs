using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button openCardButton;   // "Card"
    [SerializeField] private GameObject cardGroup;    // 패널
    [SerializeField] private Image cardImage;         // 카드 이미지
    [SerializeField] private Button cardImageButton;  // 카드 이미지 위 버튼

    [Header("Refs")]
    [SerializeField] private AttackController attackController;

    [Header("Resources 폴더")]
    [SerializeField] private string resourcesFolder = "my_asset";

    private ICardPattern _pattern; // ✅ 누락됐던 필드

    private void Awake()
    {
        if (openCardButton)    openCardButton.onClick.AddListener(OpenPanel);
        if (cardImageButton)   cardImageButton.onClick.AddListener(OnCardImageClicked); // ✅ 메서드명 일치

        if (cardGroup) cardGroup.SetActive(false);
    }

    private void OnEnable()
    {
        // 인벤토리/DB에서 현재 선택 카드명 가져오기 (예: "Card1")
        string cardName = (ItemDatabase.Instance != null && ItemDatabase.Instance.collectedItems.Count > 0)
                            ? ItemDatabase.Instance.collectedItems[0]
                            : "Card1";

        // 해당 타입의 스크립트 동적 생성하여 ICardPattern 읽기
        var type = System.Type.GetType(cardName) ?? FindTypeByName(cardName);
        if (type != null)
        {
            var go = new GameObject($"_CardProvider_{cardName}");
            var comp = go.AddComponent(type) as ICardPattern;
            _pattern = comp;

            // 카드 이미지 세팅
            if (cardImage && _pattern != null)
            {
                var sprite = Resources.Load<Sprite>(_pattern.CardImagePath);
                cardImage.sprite = sprite;
                cardImage.preserveAspect = true;
                cardImage.raycastTarget = true;
            }
        }
        else
        {
            Debug.LogWarning($"[CardController] 카드 타입을 찾지 못함: {cardName}");
        }
    }

    private void OnDisable()
    {
        // 임시 생성한 _CardProvider_* 정리
        var temp = GameObject.Find($"_CardProvider_{_pattern?.GetType().Name}");
        if (temp) Destroy(temp);
    }

    private void OpenPanel()
    {
        if (cardGroup) cardGroup.SetActive(true);
    }

    // ✅ 버튼 클릭 시 호출되는 실제 메서드
    private void OnCardImageClicked()
    {
        if (_pattern == null || attackController == null)
        {
            Debug.LogError("[CardController] _pattern 또는 attackController가 없음");
            return;
        }

        // 패턴/타이밍을 사용해서 오른쪽(Enemy) 패널에 표시
        var timings = _pattern.Timings ?? new float[16];
        attackController.ShowPattern(_pattern.Pattern16, timings, AttackController.Panel.Enemy);

        // 턴 종료 예약 (표시 연출이 모두 끝난 뒤)
        float total = attackController.GetSequenceDuration(timings);
        TurnManager.Instance.Invoke(nameof(TurnManager.EndPlayerTurn), total);

        if (cardGroup) cardGroup.SetActive(false);
    }

    // Type 찾기 보조
    private static System.Type FindTypeByName(string typeName)
    {
        var asm = typeof(CardController).Assembly;
        foreach (var t in asm.GetTypes())
            if (t.Name == typeName) return t;
        return null;
    }
}
