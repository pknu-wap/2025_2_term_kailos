// Enemy1.cs
using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    [Header("Identity")]
    public string enemyName = "Enemy1";

    [Header("Stats")]
    public int maxHP = 60;
    public int currentHP = 60;
    public int attack = 8;
    public int defense = 3;
    public int speed = 4;

    [Header("Cards (optional for later)")]
    // 추후 AI/행동 로직에서 사용할 카드 ID들(필요 없으면 비워둬도 됨)
    public string[] deck = new string[] { "Card1", "Card2", "Card3" };

    // 에디터에서 값 바꿨을 때 정합성 보정
    private void OnValidate()
    {
        maxHP = Mathf.Max(1, maxHP);
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        attack = Mathf.Max(0, attack);
        defense = Mathf.Max(0, defense);
        speed = Mathf.Max(0, speed);
    }

    [ContextMenu("Reset HP to Max")]
    private void ResetHPToMax()
    {
        currentHP = maxHP;
    }
}
