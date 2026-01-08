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
    public float statScaling = 0.5f; // Damage = base + (stat * scaling)

    [Header("Energy Cost")]
    public int energyCost = 10;

    [Header("Defensive")]
    public bool isDefensive = false;
    public int defenseBonus = 10;
    public int healAmount = 0;

    [Header("Special Effects")]
    public bool guaranteedCrit = false;
    public float critChanceBonus = 0f;
    public bool ignoreDefense = false;

    public int CalculateDamage()
    {
        if (PlayerStats.Instance == null) return baseDamage;

        int statValue = PlayerStats.Instance.Get(scalingStat);
        int totalDamage = baseDamage + Mathf.RoundToInt(statValue * statScaling);

        // Add equipment bonuses
        //if (EquipmentManager.Instance != null)
        //{
        //    totalDamage += EquipmentManager.Instance.GetTotalDamageBonus();
        //}

        return totalDamage;
    }
}
