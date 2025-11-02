using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ItemSlotUI
{
    public Text nameText;      // 아이템 이름 표시용
    public Text quantityText;  // 수량 표시용
    public Text descText;      // 설명 표시용
}

public class InventoryDisplay : MonoBehaviour
{
    [Header("슬롯 6개 연결 (Inspector에서 드래그)")]
    public ItemSlotUI[] slots;

    [Header("아이템 데이터 JSON 파일 이름 (Resources 폴더 기준)")]
    public string jsonFileName = "items"; // 예: Resources/items.json

    private InventoryData inventoryData;

    private void Start()
    {
        LoadItemsFromJson();
        DisplayItems();
    }

    /// <summary>
    /// JSON 파일을 불러와 InventoryData로 변환
    /// </summary>
    private void LoadItemsFromJson()
    {
        TextAsset json = Resources.Load<TextAsset>(jsonFileName);

        if (json == null)
        {
            Debug.LogError($"❌ {jsonFileName}.json 파일을 찾을 수 없습니다! Resources 폴더 안에 위치해야 합니다.");
            return;
        }

        // JSON 문자열 → InventoryData 객체로 변환
        inventoryData = JsonUtility.FromJson<InventoryData>(json.text);

        if (inventoryData == null || inventoryData.items == null)
        {
            Debug.LogError("⚠️ JSON 파싱 실패 또는 'items' 배열이 비어 있습니다!");
            return;
        }

        Debug.Log($"✅ {inventoryData.items.Length}개의 아이템 데이터를 성공적으로 불러왔습니다!");
    }

    /// <summary>
    /// 인벤토리 UI 슬롯에 데이터 표시
    /// </summary>
    public void DisplayItems()
    {
        if (inventoryData == null || inventoryData.items == null)
        {
            Debug.LogWarning("⚠️ InventoryData가 비어 있습니다. 표시할 아이템이 없습니다.");
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventoryData.items.Length)
            {
                var item = inventoryData.items[i];

                if (slots[i].nameText) slots[i].nameText.text = item.name;
                if (slots[i].quantityText) slots[i].quantityText.text = $"x{item.quantity}";
                if (slots[i].descText) slots[i].descText.text = item.description;

                // 콘솔 출력용 (디버깅)
                Debug.Log($"[InventoryDisplay] 로드됨 → {item.name} x{item.quantity} | {item.description}");
            }
            else
            {
                // 남는 슬롯은 비워줌
                if (slots[i].nameText) slots[i].nameText.text = "";
                if (slots[i].quantityText) slots[i].quantityText.text = "";
                if (slots[i].descText) slots[i].descText.text = "";
            }
        }
    }
}
