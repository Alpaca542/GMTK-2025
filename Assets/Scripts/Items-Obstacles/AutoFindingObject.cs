using UnityEngine;

public class AutoFindingObject : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;
    public float detectionRadius = 10f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!target)
        {
            target = GameObject.FindAnyObjectByType<PlainController>().transform;
            if (target == null)
            {
                Debug.LogWarning("No target found with tag 'Player'. Please assign a target.");
            }
        }
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distanceToTarget = Vector2.Distance(rb.position, target.position);

        if (distanceToTarget > detectionRadius)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        transform.right = target.position - transform.position;
        transform.Rotate(0, 0, 180f);
        Vector2 direction = ((Vector2)target.position - rb.position).normalized;
        rb.linearVelocity = direction * speed;
    }
}