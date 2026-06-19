using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class Projectile : NetworkBehaviour
{
    private readonly float explosionRadius = 2f;
    [SerializeField] private LayerMask destroyableLayer;

    private Vector3 previousPosition;
    private readonly float rotationDuration = 2.5f;
    private float ratio = 0f;
    private Quaternion startRotation;
    private Quaternion endRotation;
    private float time = 0f;
    private float lastDirectionX = 0f;

    private void Start()
    {
        previousPosition = transform.position;
        startRotation = transform.rotation;
        endRotation = transform.rotation;
    }

    private void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector3 movementDirection = currentPosition - previousPosition;

        if (movementDirection.x > 0 && lastDirectionX <= 0)
        {
            startRotation = Quaternion.Euler(0, 0, 0);
            endRotation = Quaternion.Euler(0, 0, 180);
            ratio = 0f;
            lastDirectionX = 1f;
        }
        else if (movementDirection.x < 0 && lastDirectionX >= 0)
        {
            startRotation = Quaternion.Euler(0, 0, 0);
            endRotation = Quaternion.Euler(0, 0, -180);
            ratio = 0f;
            lastDirectionX = -1f;
        }

        previousPosition = currentPosition;
        ratio = Mathf.Clamp01(ratio + Time.deltaTime / rotationDuration);
        transform.rotation = Quaternion.Slerp(startRotation, endRotation, ratio);

        if (!IsServer) return;

        time += Time.deltaTime;
        if (time > 10f)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("Barrier"))
        {
            Destroy(collision.gameObject);
        }

        Explode(transform.position);
        GetComponent<NetworkObject>().Despawn();
    }

    private void Explode(Vector3 explosionCenter)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(explosionCenter, explosionRadius, destroyableLayer);

        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                Destroy(collider.gameObject);
            }
        }
    }
}
