using UnityEngine;
using UnityEngine.EventSystems;

public class WorldItem : MonoBehaviour, IPointerClickHandler
{
    public ItemData data;
    public int quantity = 1;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (data == null)
        {
            Debug.LogWarning($"[WorldItem] {name} has no ItemData assigned.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[WorldItem] InventoryManager not available.");
            return;
        }

        InventoryManager.Instance.AddItem(data, quantity);

        if (LootNotificationUI.Instance != null)
            LootNotificationUI.Instance.ShowLoot(data);

        Destroy(gameObject);
    }
}
