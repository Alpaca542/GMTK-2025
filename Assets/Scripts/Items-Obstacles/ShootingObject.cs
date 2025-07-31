using UnityEngine;

public class ShootingObject : MonoBehaviour
{
    public Transform player;
    public GameObject bulletPrefab;
    public float shootingRange = 10f;
    public float shootCooldown = 1f;
    public float bulletSpeed = 10f;

    private float lastShootTime;

    void Update()
    {
        if (player == null) return;

        // Look at player
        Vector3 direction = player.position - transform.position;
        direction.y = 0; // Optional: keep rotation horizontal
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        // Check if player is in range
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= shootingRange && Time.time >= lastShootTime + shootCooldown)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward, Quaternion.identity);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = transform.forward * bulletSpeed;
        }
    }
}