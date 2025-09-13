using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance; // 싱글톤
    public List<string> collectedItems = new List<string>(); // 습득한 아이템 이름 리스트

    void Awake()
    {
        // 싱글톤 생성
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환시에도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 아이템 추가
    public void AddItem(string itemName)
    {
        if (!collectedItems.Contains(itemName))
        {
            collectedItems.Add(itemName);
            Debug.Log($"[ItemDatabase] {itemName} 추가됨");
        }
    }
}
