using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public bool useable;
    public StatType affectedStat;
    public int statAmount;

    public UnityEvent onUse;
}
