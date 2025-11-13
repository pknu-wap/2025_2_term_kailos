using UnityEngine;

public enum ItemType
{
    Consumable,
    Key,
    Etc
}

[CreateAssetMenu(menuName = "Items/Item", fileName = "NewItem")]
public class ItemSO : ScriptableObject
{
    [Header("ID & Meta")]
    public string id;            // JSON에서 쓸 고유 이름 (예: "potion")
    public string displayName;   // 인벤토리에 표시될 이름
    public ItemType type;

    [Header("설명 & 비주얼")]
    [TextArea] public string description; // 자세한 설명
    public Sprite icon;         // 인벤토리 아이콘

    [Header("기본 설정 (선택사항)")]
    public int defaultQuantity = 0;
}
