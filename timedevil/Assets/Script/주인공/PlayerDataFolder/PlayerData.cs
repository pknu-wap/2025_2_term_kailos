// PlayerData.cs
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [Header("Identity")]
    public string playerName = "Player";

    [Header("Stats")]
    public int maxHP = 100;
    public int currentHP = 100;
    public int attack = 10;
    public int defense = 5;
    public int speed = 5;     // ���� ���� � ���

    [Header("Emotion Counter")]
    public int emotionPositive = 0;   // ����
    public int emotionNegative = 0;   // ����

    public bool IsDead => currentHP <= 0;

    /// <summary>�ʱⰪ ����(���ǿ�)</summary>
    public void InitDefaults(
        string name = "Player", int hp = 100, int atk = 10, int def = 5, int spd = 5)
    {
        playerName = string.IsNullOrEmpty(name) ? "Player" : name;
        maxHP = Mathf.Max(1, hp);
        currentHP = maxHP;
        attack = Mathf.Max(0, atk);
        defense = Mathf.Max(0, def);
        speed = Mathf.Max(0, spd);
        emotionPositive = 0;
        emotionNegative = 0;
    }
}
