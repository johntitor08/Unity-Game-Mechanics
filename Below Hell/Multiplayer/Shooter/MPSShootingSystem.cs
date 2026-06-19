using Unity.Netcode;
using UnityEngine;

public class MPSShootingSystem : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20;
    private float lastFiredTime = 0;
    public float fireFrequency = 0.5f;
    public bool isBiggerBullet = false;

    private void Update()
    {
        if (IsOwner)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePosition - transform.position).normalized;
            transform.up = direction;

            if (Input.GetButtonDown("Fire1") && lastFiredTime > fireFrequency)
            {
                FireServerRpc(firePoint.position, transform.rotation, isBiggerBullet);
                lastFiredTime = 0;
            }
        }

        lastFiredTime += Time.deltaTime;
    }

    [ServerRpc]
    private void FireServerRpc(Vector3 position, Quaternion rotation, bool biggerBullet)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, rotation);
        bullet.transform.localScale = biggerBullet ? new Vector3(0.3f, 0.3f, 0.3f) : new Vector3(0.15f, 0.15f, 0.15f);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = rotation * Vector3.up * bulletSpeed;
        bullet.GetComponent<NetworkObject>().Spawn();
    }
}
