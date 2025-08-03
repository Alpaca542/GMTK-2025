using UnityEngine;

public class ShootingObject : MonoBehaviour
{
    public Transform player;
    public GameObject bulletPrefab;
    public float shootingRange = 10f;
    public float shootCooldown = 1f;
    public float bulletSpeed = 10f;
    public bool lookAtPlayer = true;

    private float lastShootTime;

    void Update()
    {
        if (player == null) return;

        // Look at player (optional)
        if (lookAtPlayer)
        {
            Vector2 direction = player.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // Check if player is in range
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= shootingRange && Time.time >= lastShootTime + shootCooldown)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position + (Vector3)transform.right, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = transform.right * bulletSpeed;
        }
    }
}