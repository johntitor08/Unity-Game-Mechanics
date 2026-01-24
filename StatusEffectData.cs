using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffect", menuName = "Combat/Status Effect")]
public class StatusEffectData : ScriptableObject
{
    [Header("Basic Info")]
    public StatusEffectType effectType;
    public string effectName = "Poison";
    public Sprite icon;
    [TextArea] public string description = "Takes damage over time";

    [Header("Visual")]
    public Color effectColor = Color.green;
    public GameObject particleEffectPrefab;
    public AudioClip applySound;
    public AudioClip tickSound;

    [Header("Duration")]
    public float duration = 5f;
    public bool isPermanent = false;

    [Header("Tick Settings")]
    public bool hasTicks = true;
    public float tickInterval = 1f; // Damage/heal every second
    public int tickDamage = 5; // Damage per tick (negative for healing)

    [Header("Stat Modifiers")]
    public StatModifier[] statModifiers;

    [Header("Behavior")]
    public bool canStack = false;
    public int maxStacks = 1;
    public bool refreshOnReapply = true;
    public bool isPurgeableByPlayer = true;
    public bool isDebuff = true;

    [Header("Special Effects")]
    public bool preventActions = false; // Like Stun
    public bool preventMovement = false;
    public float damageMultiplier = 1f; // Multiply damage dealt
    public float damageReduction = 0f; // Reduce damage taken (0-1)

    [System.Serializable]
    public class StatModifier
    {
        public StatType statType;
        public int amount;
        public bool isPercentage;
    }
}
