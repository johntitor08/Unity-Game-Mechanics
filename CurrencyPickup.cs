using UnityEngine;

public class CurrencyPickup : MonoBehaviour
{
    [Header("Currency")]
    public CurrencyType currencyType = CurrencyType.Gold;
    public int amount = 10;

    [Header("Pickup Settings")]
    public bool autoPickup = true;
    public float pickupRange = 1f;
    public bool destroyOnPickup = true;

    [Header("Visual")]
    public GameObject pickupEffect;
    public AudioClip pickupSound;

    private bool isPickedUp = false;

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
        if (!autoPickup && !isPickedUp)
        {
            // Check for manual pickup
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= pickupRange && Input.GetKeyDown(KeyCode.F))
                {
                    Pickup();
                }
            }
        }
    }

    void Pickup()
    {
        if (isPickedUp) return;
        isPickedUp = true;

        // Add currency
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.Add(currencyType, amount);
        }

        // Visual effect
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        // Sound
        if (pickupSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position);
        }

        // Destroy or hide
        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
