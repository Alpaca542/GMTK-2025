using UnityEngine;

public class ShootingObject : MonoBehaviour
{
    public Transform target;
    public GameObject bulletPrefab;
    public float shootingRange = 10f;
    public float shootCooldown = 1f;
    public float bulletSpeed = 10f;
    public bool lookAtTarget = true;

    private float lastShootTime;

    void Update()
    {
        if (target == null) return;

        // Check if target is in range
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= shootingRange && Time.time >= lastShootTime + shootCooldown)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Quaternion lookRotation = Quaternion.LookRotation(
            target.transform.position - transform.position,
            transform.TransformDirection(Vector3.up)
        );
        bullet.transform.rotation = new Quaternion(0, 0, lookRotation.z, lookRotation.w);

        Vector2 direction = (target.position - transform.position).normalized;
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
        }
    }
}