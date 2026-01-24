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
    public float popScaleMultiplier = 1.2f;

    private readonly Queue<GameObject> notificationPool = new();
    private readonly List<GameObject> activeNotifications = new();

    void Awake()
    {
        Instance = this;
    }

    // Show multiple currency changes sequentially
    public void Show(Dictionary<CurrencyType, int> changes, float delayBetween = 0.1f)
    {
        if (changes == null || changes.Count == 0) return;

        foreach (var kvp in changes)
        {
            StartCoroutine(ShowWithDelay(kvp.Key, kvp.Value, delayBetween));
        }
    }

    private System.Collections.IEnumerator ShowWithDelay(CurrencyType type, int amount, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowSingle(type, amount);
    }

    private void ShowSingle(CurrencyType type, int amount)
    {
        GameObject notification = GetNotification();

        if (notification.TryGetComponent<TextMeshProUGUI>(out var text))
        {
            var currencyInfo = CurrencyManager.Instance.GetCurrencyInfo(type);
            Color color = currencyInfo != null ? currencyInfo.displayColor : Color.white;
            string colorHex = ColorUtility.ToHtmlStringRGB(color);
            string prefix = amount >= 0 ? "+" : "-";
            text.text = $"<color=#{colorHex}>{prefix}{Mathf.Abs(amount)} {type}</color>";
        }

        notification.SetActive(true);
        notification.transform.localScale = Vector3.zero;
        activeNotifications.Add(notification);
        StartCoroutine(AnimateNotification(notification));
    }

    private GameObject GetNotification()
    {
        if (notificationPool.Count > 0)
            return notificationPool.Dequeue();

        return Instantiate(notificationPrefab, notificationParent);
    }

    private System.Collections.IEnumerator AnimateNotification(GameObject notification)
    {
        float elapsed = 0f;
        Vector3 startPos = notification.transform.position;
        var text = notification.GetComponent<TextMeshProUGUI>();
        Color startColor = text != null ? text.color : Color.white;

        while (elapsed < notificationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / notificationDuration;
            notification.transform.position = startPos + Vector3.up * (moveSpeed * t);

            // Fade out
            if (text != null)
            {
                Color color = startColor;
                color.a = 1f - t;
                text.color = color;
            }

            // Pop scale animation (in then out)
            float scale = Mathf.Sin(t * Mathf.PI) * popScaleMultiplier;
            notification.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        // Return to pool
        notification.SetActive(false);
        activeNotifications.Remove(notification);
        notificationPool.Enqueue(notification);
    }
}
