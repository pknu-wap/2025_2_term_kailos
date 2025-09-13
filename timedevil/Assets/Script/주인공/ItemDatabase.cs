using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance; // �̱���
    public List<string> collectedItems = new List<string>(); // ������ ������ �̸� ����Ʈ

    void Awake()
    {
        // �̱��� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ�ÿ��� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ������ �߰�
    public void AddItem(string itemName)
    {
        if (!collectedItems.Contains(itemName))
        {
            collectedItems.Add(itemName);
            Debug.Log($"[ItemDatabase] {itemName} �߰���");
        }
    }
}
