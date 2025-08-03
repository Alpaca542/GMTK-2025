using UnityEngine;

public class Shark : MonoBehaviour
{
    public float jumpForce = 500f;
    public float jumpInterval = 3f;
    public float upwardForce = 300f;

    private Vector3 startPos;
    private Rigidbody2D rb;
    private Transform player;
    private float timer;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        timer = jumpInterval;
    }

    void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            JumpTowardsPlayer();
            timer = jumpInterval;
        }
    }

    void JumpTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Keep horizontal only

        rb.linearVelocity = Vector2.zero; // Reset velocity
        rb.AddForce(direction * jumpForce + Vector2.up * upwardForce);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            transform.position = startPos;
            rb.linearVelocity = Vector2.zero;
        }
    }
}
