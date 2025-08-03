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
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        Vector2 direction = (target.position - transform.position).normalized;
        if (bulletRb != null)
        {
            bulletRb.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            bullet.transform.Rotate(0, 0, -90f);
        }
        if (bulletRb != null)
        {
            bulletRb.AddForce(direction * bulletSpeed);
        }
    }
}