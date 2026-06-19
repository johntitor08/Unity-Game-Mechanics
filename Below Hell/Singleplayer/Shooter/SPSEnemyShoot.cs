using UnityEngine;

public class SPSEnemyShoot : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shootInterval = 1f;
    [SerializeField] private float bulletSpeed = 20f;
    private Transform target;
    private float shootTimer;

    private void Start()
    {
        shootTimer = shootInterval;
        target = GameObject.FindWithTag("Player").GetComponent<Transform>();

    }

    private void Update()
    {
        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootInterval;

        }

    }

    private void Shoot()
    {
        if (target && !target.GetComponent<SPSPlayerComponents>().isInvisible)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity, transform.parent.parent);
            Vector3 direction = (target.transform.position - transform.position).normalized;
            Rigidbody2D projectileRigidbody = bullet.GetComponent<Rigidbody2D>();
            projectileRigidbody.linearVelocity = direction * bulletSpeed;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            bullet.transform.rotation = transform.rotation;
            Destroy(bullet, 3f);

        }

    }

}