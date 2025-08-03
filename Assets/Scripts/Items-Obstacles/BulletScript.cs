using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private GameObject particleEffectPrefab;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * speed;

        // Destroy bullet after 5 seconds if it doesn't hit anything
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Spawn particles at bullet position
            if (particleEffectPrefab != null)
            {
                Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
            }

            // Destroy the bullet
            Destroy(gameObject);
        }
    }
}