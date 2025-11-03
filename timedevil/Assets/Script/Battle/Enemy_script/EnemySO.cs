using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Enemy SO", fileName = "EnemySO")]
public class EnemySO : ScriptableObject
{
    [Header("Identity")]
    public string enemyId = "Enemy1";
    public string displayName = "Enemy 1";

    [Header("Base Stats")]
    public int baseHP = 60;
    public int baseATK = 8;
    public int baseDEF = 3;
    public int baseSPD = 6;

    [Header("Optional Deck (IDs)")]
    public string[] deckIds;
}
