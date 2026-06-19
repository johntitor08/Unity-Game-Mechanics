using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MPSPlayerMovement : NetworkBehaviour
{
    private Rigidbody2D rb;
    private float horMov;
    private float verMov;
    [SerializeField] private float horSpeed;
    [SerializeField] private float verSpeed;
    private float timeSpeedy;
    private Vector2 movement;
    private bool isSpeedy = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        timeSpeedy = 0f;
    }

    private void Update()
    {
        if (IsOwner)
        {
            horMov = Input.GetAxis("Horizontal");
            verMov = Input.GetAxis("Vertical");

            if (timeSpeedy <= 0)
            {
                movement = new Vector2(horMov * horSpeed, verMov * verSpeed);
                rb.linearVelocity = movement;
                isSpeedy = false;
            }
            else
            {
                movement = new Vector2(horMov * horSpeed * 2, verMov * verSpeed * 2);
                rb.linearVelocity = movement;
                isSpeedy = true;
            }

            timeSpeedy -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsOwner)
        {
            if (collision.gameObject.CompareTag("Speed"))
            {
                timeSpeedy = 5f;

                if (isSpeedy)
                {
                    timeSpeedy += 5f;
                }

                Destroy(collision.gameObject);
            }
        }
    }
}
