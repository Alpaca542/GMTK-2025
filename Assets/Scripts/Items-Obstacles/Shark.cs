using UnityEngine;

public class Shark : MonoBehaviour
{
    public float jumpForce = 10f;
    public float jumpInterval = 3f;
    public float jumpHeight = 2f;

    private Vector3 startPos;
    private Rigidbody2D rb;
    private Transform player;
    private float timer;
    private bool isJumping;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        timer = jumpInterval;
        isJumping = false;
    }

    void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;

        if (!isJumping && timer <= 0f)
        {
            JumpTowardsPlayer();
            isJumping = true;
        }
    }

    void JumpTowardsPlayer()
    {
        Vector2 direction = player.position - startPos;
        direction.y = 0; // Only move horizontally towards player
        direction = direction.normalized;
        Vector2 jumpVector = new Vector2(direction.x * jumpForce, jumpHeight);
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(jumpVector, ForceMode2D.Impulse);
        Invoke(nameof(ResetShark), 2f);
    }

    void ResetShark()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = startPos;
        timer = jumpInterval;
        isJumping = false;
    }
}
