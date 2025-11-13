using UnityEngine;

[System.Serializable]
public class InventoryItemEntry
{
    public string id;      // ItemSO.id (예: "potion")
    public int quantity;   // 현재 가지고 있는 개수
}

[System.Serializable]
public class InventorySaveData
{
    public InventoryItemEntry[] items;
}
