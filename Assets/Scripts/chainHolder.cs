using UnityEngine;

public class chainHolder : MonoBehaviour
{
    public Rigidbody2D planeRb;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        transform.rotation = planeRb.transform.rotation;
        Vector2 targetPos = planeRb.position;
        rb.MovePosition(targetPos);
    }
}
