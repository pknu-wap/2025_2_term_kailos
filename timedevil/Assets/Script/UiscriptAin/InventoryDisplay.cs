using UnityEngine;
using UnityEngine.UI;

#region UI 슬롯 구조
[System.Serializable]
public class ItemSlotUI
{
    public Text nameText;      // 아이템 이름
    public Text quantityText;  // 수량
    public Text descText;      // 설명
    public Image iconImage;    // 아이콘 이미지 (없으면 비워둬도 됨)
}
#endregion

public class InventoryDisplay : MonoBehaviour
{
    [Header("슬롯 6개 연결 (Inspector에서 드래그)")]
    public ItemSlotUI[] slots;

    [Header("아이템 데이터 JSON 파일 이름 (Resources/ 파일명만)")]
    public string jsonFileName = "items";   // Resources/items.json

    [Header("아이템 데이터베이스(SO)")]
    public ItemDatabaseSO itemDatabase;     // 🔥 여기에 ItemDatabase SO 드래그

    [Header("페이지 설정")]
    [Tooltip("0=첫 페이지, 1=두 번째 페이지...")]
    [SerializeField] private int pageIndex = 0;
    [SerializeField] private int pageSize = 6;       // 한 페이지에 표시할 개수(슬롯 수와 동일 권장)

    private InventorySaveData inventoryData;

    private void Start()
    {
        LoadItemsFromJson();
        DisplayCurrentPage();
    }

    /// <summary>Resources/{jsonFileName}.json을 읽어 InventorySaveData로 역직렬화</summary>
    private void LoadItemsFromJson()
    {
        TextAsset json = Resources.Load<TextAsset>(jsonFileName);
        if (json == null)
        {
            Debug.LogError($"❌ {jsonFileName}.json 파일을 찾을 수 없습니다! (Resources 폴더 확인)");
            return;
        }

        inventoryData = JsonUtility.FromJson<InventorySaveData>(json.text);
        if (inventoryData == null || inventoryData.items == null)
        {
            Debug.LogError("⚠️ JSON 파싱 실패 또는 'items' 배열이 비었습니다.");
            return;
        }

        Debug.Log($"✅ {inventoryData.items.Length}개의 인벤토리 데이터를 성공적으로 불러왔습니다!");
    }

    /// <summary>현재 pageIndex 기준으로 페이지 내용을 슬롯에 표시</summary>
    public void DisplayCurrentPage()
    {
        if (inventoryData == null || inventoryData.items == null)
        {
            Debug.LogWarning("⚠️ InventorySaveData가 비어 있습니다. 표시할 아이템이 없습니다.");
            ClearAllSlots();
            return;
        }

        int start = Mathf.Max(0, pageIndex * pageSize);
        int end = Mathf.Min(start + pageSize, inventoryData.items.Length);

        for (int i = 0; i < slots.Length; i++)
        {
            int dataIdx = start + i;

            if (dataIdx >= start && dataIdx < end)
            {
                var entry = inventoryData.items[dataIdx];

                // 🔥 ItemDatabase에서 정의 가져오기
                ItemSO def = itemDatabase != null
                    ? itemDatabase.GetById(entry.id)
                    : null;

                string displayName = def != null ? def.displayName : entry.id;
                string description = def != null ? def.description : "";
                Sprite icon = def != null ? def.icon : null;

                if (slots[i].nameText) slots[i].nameText.text = displayName;
                if (slots[i].quantityText) slots[i].quantityText.text = $"x{entry.quantity}";
                if (slots[i].descText) slots[i].descText.text = description;

                if (slots[i].iconImage)
                {
                    slots[i].iconImage.sprite = icon;
                    slots[i].iconImage.enabled = icon != null;
                }

                Debug.Log($"[InventoryDisplay p{pageIndex}] {entry.id} x{entry.quantity} | {description}");
            }
            else
            {
                // 남는 칸 클리어
                ClearSlot(slots[i]);
            }
        }
    }

    /// <summary>외부(페이지 매니저)에서 페이지를 바꿀 때 호출</summary>
    public void SetPage(int newPageIndex)
    {
        if (newPageIndex < 0) newPageIndex = 0;
        pageIndex = newPageIndex;
        DisplayCurrentPage();
    }

    /// <summary>슬롯 전부 비우기</summary>
    private void ClearAllSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            ClearSlot(slots[i]);
        }
    }

    private void ClearSlot(ItemSlotUI slot)
    {
        if (slot == null) return;

        if (slot.nameText) slot.nameText.text = "";
        if (slot.quantityText) slot.quantityText.text = "";
        if (slot.descText) slot.descText.text = "";
        if (slot.iconImage)
        {
            slot.iconImage.sprite = null;
            slot.iconImage.enabled = false;
        }
    }
}
