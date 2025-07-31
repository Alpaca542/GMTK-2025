using UnityEngine;

public class AutoFindingObject : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }
}