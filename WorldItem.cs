using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class WorldItem : MonoBehaviour, IPointerClickHandler
{
    public ItemData data;
    public int quantity = 1;

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryManager.Instance.AddItem(data, quantity);
        Destroy(gameObject);
    }
}
