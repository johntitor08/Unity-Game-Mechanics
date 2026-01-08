using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CurrencyNotificationUI : MonoBehaviour
{
    public static CurrencyNotificationUI Instance;

    [Header("Notification Settings")]
    public GameObject notificationPrefab;
    public Transform notificationParent;
    public float notificationDuration = 2f;
    public float moveSpeed = 50f;

    private Queue<GameObject> notificationPool = new();
    private List<GameObject> activeNotifications = new();

    void Awake()
    {
        Instance = this;
    }

    public void Show(string message, Color color)
    {
        GameObject notification = GetNotification();

        var text = notification.GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = message;
            text.color = color;
        }

        notification.SetActive(true);
        activeNotifications.Add(notification);

        StartCoroutine(AnimateNotification(notification));
    }

    GameObject GetNotification()
    {
        if (notificationPool.Count > 0)
        {
            return notificationPool.Dequeue();
        }
        else
        {
            return Instantiate(notificationPrefab, notificationParent);
        }
    }

    System.Collections.IEnumerator AnimateNotification(GameObject notification)
    {
        float elapsed = 0f;
        Vector3 startPos = notification.transform.position;

        var text = notification.GetComponent<TextMeshProUGUI>();
        Color startColor = text.color;

        while (elapsed < notificationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / notificationDuration;

            // Move up
            notification.transform.position = startPos + Vector3.up * (moveSpeed * elapsed);

            // Fade out
            if (text != null)
            {
                Color color = startColor;
                color.a = 1f - t;
                text.color = color;
            }

            yield return null;
        }

        // Return to pool
        notification.SetActive(false);
        activeNotifications.Remove(notification);
        notificationPool.Enqueue(notification);
    }
}
