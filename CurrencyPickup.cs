using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CurrencyPickupData
{
    public CurrencyType type;
    public int amount = 10;
}

public class CurrencyPickup : MonoBehaviour
{
    [Header("Currency")]
    public List<CurrencyPickupData> currencies = new();

    [Header("Pickup Settings")]
    public bool autoPickup = true;
    public float pickupRange = 1f;
    public bool destroyOnPickup = true;

    [Header("Visual & Audio")]
    public GameObject pickupEffect;
    public AudioClip pickupSound;

    [Header("Optional Event")]
    public System.Action<Dictionary<CurrencyType, int>> OnPickedUp;

    private bool isPickedUp = false;
    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            playerTransform = player.transform;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp) return;

        if (other.CompareTag("Player") && autoPickup)
        {
            Pickup();
        }
    }

    void Update()
    {
        if (!autoPickup && !isPickedUp && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            if (distance <= pickupRange && Input.GetKeyDown(KeyCode.F))
            {
                Pickup();
            }
        }
    }

    void Pickup()
    {
        if (isPickedUp) return;
        isPickedUp = true;
        var amounts = new Dictionary<CurrencyType, int>();

        foreach (var data in currencies)
        {
            if (data.amount > 0)
                amounts[data.type] = data.amount;
        }

        if (CurrencyManager.Instance != null && amounts.Count > 0)
        {
            CurrencyManager.Instance.AddMultiple(amounts);
        }

        OnPickedUp?.Invoke(amounts);

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        if (pickupSound != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position);

        if (destroyOnPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
