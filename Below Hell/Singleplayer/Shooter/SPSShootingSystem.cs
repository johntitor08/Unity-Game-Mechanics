using UnityEngine;

public class SPSShootingSystem : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20;
    private float lastFiredTime = 0;
    public float fireFrequency = 0.5f;
    public bool isBiggerBullet = false;

    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - transform.position).normalized;
        transform.up = direction;

        if (Input.GetButtonDown("Fire1") && lastFiredTime >= fireFrequency)
        {
            Fire();
            lastFiredTime = 0;

        }

        lastFiredTime += Time.deltaTime;

    }

    private void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, transform.rotation, transform.parent.parent);

        if (!isBiggerBullet)
        {
            bullet.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

        }
        else
        {
            bullet.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        }

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.up * bulletSpeed;

    }

}
