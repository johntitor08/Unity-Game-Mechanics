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
    public bool isRoundBased = false;
    public int durationRounds = 3;

    [Header("Tick Settings")]
    public bool hasTicks = true;
    public float tickInterval = 1f;
    public int tickDamage = 5;

    [Header("Stat Modifiers")]
    public StatModifier[] statModifiers;

    [Header("Behavior")]
    public bool canStack = false;
    public int maxStacks = 1;
    public bool refreshOnReapply = true;
    public bool isPurgeableByPlayer = true;
    public bool isDebuff = true;

    [Header("Special Effects")]
    public bool preventActions = false;
    public bool preventMovement = false;
    public float damageMultiplier = 1f;
    public float damageReduction = 0f;

    [System.Serializable]
    public class StatModifier
    {
        public StatType statType;
        public int amount;
        public bool isPercentage;
    }
}
