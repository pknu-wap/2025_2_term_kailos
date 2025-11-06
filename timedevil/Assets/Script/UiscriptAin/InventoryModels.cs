using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string name;
    public int quantity;
    public string description;
    public string spritePath;
}

[System.Serializable]
public class InventoryData
{
    public ItemData[] items;
}
