using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float maxLifetime = 2f;
    public float rayLength = 2f;
    public LayerMask hitMask;

    private Rigidbody2D rb;
    float lifetime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;
        float distance = velocity.magnitude * Time.fixedDeltaTime;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, velocity.normalized, distance, hitMask);
        if (hit.collider)
        {
            Debug.Log("Bullet hit: " + hit.collider.name);
            Destroy(gameObject);
            return;
        }

        lifetime += Time.fixedDeltaTime;
        if (lifetime >= maxLifetime) Destroy(gameObject);
    }
}
