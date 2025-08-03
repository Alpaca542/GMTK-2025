using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector2 initialSpeed = new Vector2(100f, 0f);
    public float maxLifetime = 2f;
    public float rayLength = 2f;
    public LayerMask hitMask;

    float lifetime = 0f;

    void FixedUpdate()
    {
        Vector2 velocity = initialSpeed;
        float distance = velocity.magnitude * Time.fixedDeltaTime;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, velocity.normalized, distance, hitMask);
        if (hit.collider)
        {
            Debug.Log("Bullet hit: " + hit.collider.name);
            Destroy(gameObject);
            return;
        }

        transform.position += (Vector3)(velocity * Time.deltaTime);
        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime) Destroy(gameObject);
    }
}
