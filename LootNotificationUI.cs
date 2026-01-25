using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootNotificationUI : MonoBehaviour
{
    public static LootNotificationUI Instance;

    [Header("Notification")]
    public GameObject notificationPrefab;
    public Transform notificationParent;
    public float notificationDuration = 3f;

    private static readonly Queue<GameObject> notificationPool = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowLoot(EquipmentData equipment)
    {
        if (equipment == null) return;
        string message = $"+1 {equipment.itemName}";
        Color color = equipment.GetRarityColor();
        ShowNotification(message, color, equipment.icon);
    }

    public void ShowLoot(ItemData item)
    {
        if (item == null) return;
        string message = $"+1 {item.itemName}";
        ShowNotification(message, Color.white, item.icon);
    }

    void ShowNotification(string message, Color color, Sprite icon = null)
    {
        GameObject notification = GetNotification();
        
        if (notification.TryGetComponent<LootNotification>(out LootNotification notifScript))
        {
            notifScript.Setup(message, color, icon, notificationDuration);
        }

        notification.SetActive(true);
    }

    GameObject GetNotification()
    {
        if (notificationPool.Count > 0)
        {
            return notificationPool.Dequeue();
        }

        return Instantiate(notificationPrefab, notificationParent);
    }

    public void ReturnToPool(GameObject notification)
    {
        notification.SetActive(false);
        notificationPool.Enqueue(notification);
    }
}
