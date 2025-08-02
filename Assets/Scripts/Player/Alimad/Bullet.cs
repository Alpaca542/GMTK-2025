using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 100f;
    public float maxLifetime = 2f;
    public float rayLength = 2f;
    public LayerMask hitMask;

    float lifetime = 0f;

    void Update()
    {
        float distance = speed * Time.deltaTime;
        Vector2 direction = transform.right;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, hitMask);
        if (hit.collider)
        {
            Debug.Log("Bullet hit: " + hit.collider.name);
            Destroy(gameObject);
            return;
        }

        transform.position += (Vector3)(direction * distance);
        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime) Destroy(gameObject);
    }
}
