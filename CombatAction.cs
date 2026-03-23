using UnityEngine;

[System.Serializable]
public class CombatAction
{
    [Header("Basic Info")]
    public string actionName = "Attack";
    public string description = "A basic attack";
    public Sprite icon;

    [Header("Damage")]
    public int baseDamage = 10;
    public StatType scalingStat = StatType.Strength;
    public float statScaling = 0.5f;

    [Header("Energy Cost")]
    public int energyCost = 10;

    [Header("Defensive")]
    public bool isDefensive = false;
    public int defenseBonus = 30;
    [Tooltip("Defense stat'ının kaçta biri bonus defense'e eklenir (0.5 = %50)")]
    public float defenseStatScaling = 0.5f;
    public int healAmount = 0;

    [Header("Special Effects")]
    public bool guaranteedCrit = false;
    public float critChanceBonus = 0f;
    public bool ignoreDefense = false;

    [Header("Armor Interaction")]
    [Range(0f, 1f)]
    public float armorPenetration = 0f;

    public int CalculateDamage()
    {
        if (PlayerStats.Instance == null)
            return baseDamage;

        int statValue = PlayerStats.Instance.Get(scalingStat);
        int damage = baseDamage + Mathf.RoundToInt(statValue * statScaling);

        if (EquipmentManager.Instance != null)
            damage += EquipmentManager.Instance.GetTotalDamageBonus();

        return Mathf.Max(1, damage);
    }

    public int CalculateDefenseBonus()
    {
        int bonus = defenseBonus;

        if (PlayerStats.Instance != null)
            bonus += Mathf.RoundToInt(PlayerStats.Instance.Get(StatType.Defense) * defenseStatScaling);

        if (EquipmentManager.Instance != null)
            bonus += Mathf.RoundToInt(EquipmentManager.Instance.GetTotalDefenseBonus() * defenseStatScaling);

        return bonus;
    }
}
