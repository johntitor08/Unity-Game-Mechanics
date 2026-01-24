using UnityEngine;

[CreateAssetMenu(fileName = "StatModifierItem", menuName = "Stat/StatModifierItem")]
public class StatModifierItem : ItemData
{
    [Header("Stat Modification")]
    public StatType targetStat;
    public int modifyAmount;
    public bool modifyMaxStat = false;

    public void Use()
    {
        if (PlayerStats.Instance == null) return;

        if (modifyMaxStat)
        {
            PlayerStats.Instance.ModifyMax(targetStat, modifyAmount);
        }
        else
        {
            PlayerStats.Instance.Modify(targetStat, modifyAmount);
        }

        Debug.Log($"Used {itemName}: {targetStat} {(modifyAmount > 0 ? "+" : "")}{modifyAmount}");
    }
}
