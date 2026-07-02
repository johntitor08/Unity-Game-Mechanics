using UnityEngine;

[CreateAssetMenu(fileName = "ConsumableBuffItem", menuName = "Inventory/Consumable Buff Item")]
public class ConsumableBuffItem : ItemData
{
    [Header("Combat Buff")]
    public PlayerBuffManager.BuffType buffType = PlayerBuffManager.BuffType.Damage;
    [Tooltip("Outgoing damage multiplier (used by Damage buffs). 1.3 = +30% ATK.")]
    public float damageMultiplier = 1f;
    [Tooltip("Incoming damage reduction 0..1 (used by Defense buffs). 0.3 = take 30% less.")]
    public float damageReduction = 0f;
    [Tooltip("Number of fights the buff stays active.")]
    public int fights = 2;

    public void Use()
    {
        if (PlayerBuffManager.Instance == null)
            return;

        PlayerBuffManager.Instance.AddBuff(new PlayerBuffManager.Buff
        {
            id = string.IsNullOrEmpty(itemID) ? name : itemID,
            type = buffType,
            damageMultiplier = damageMultiplier,
            damageReduction = damageReduction,
            fightsRemaining = Mathf.Max(1, fights),
            displayName = itemName,
            icon = icon
        });

        Debug.Log($"Used {itemName}: {buffType} buff for {fights} fight(s).");
    }
}
