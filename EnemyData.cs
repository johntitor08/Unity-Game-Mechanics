using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Combat/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";
    public Sprite sprite;
    [TextArea] public string description;

    [Header("Stats")]
    public int maxHealth = 100;
    public int attack = 15;
    public int defense = 5;
    public int speed = 10;

    [Header("Rewards")]
    public int experienceReward = 50;
    public int currencyReward = 25;

    [Header("Loot")]
    public ItemData[] possibleLoot;
    [Range(0f, 1f)]
    public float[] lootChances;

    [Header("AI Behavior")]
    public bool isAggressive = true;
    public float defendChance = 0.2f;
    public float specialAttackChance = 0.3f;
}
