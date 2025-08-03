using UnityEngine;

public class Shark : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 800f;
    public float dashCooldown = 2f;
    public float detectionRange = 8f;
    public float returnSpeed = 200f;

    [Header("References")]
    public Transform waterLevel; // Optional: reference to water surface position

    private Vector3 startPos;
    private Rigidbody2D rb;
    private Transform player;
    private float cooldownTimer;
    private bool isDashing = false;
    private bool isReturning = false;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cooldownTimer = 0f;
    }

    void Update()
    {
        if (player == null) return;

        // Only dash if not currently dashing or returning
        if (!isDashing && !isReturning)
        {
            cooldownTimer -= Time.deltaTime;

            // Check if player is within range and cooldown is ready
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRange && cooldownTimer <= 0f)
            {
                DashAtPlayer();
            }
        }

        // Handle returning to start position
        if (isReturning)
        {
            ReturnToStart();
        }
    }

    void DashAtPlayer()
    {
        isDashing = true;
        cooldownTimer = dashCooldown;

        // Calculate direction to player
        Vector2 direction = (player.position - transform.position).normalized;

        // Apply dash force
        rb.linearVelocity = direction * dashSpeed;
    }

    void ReturnToStart()
    {
        // Move back to starting position
        Vector2 direction = (startPos - transform.position).normalized;
        rb.linearVelocity = direction * returnSpeed;

        // Check if close enough to start position
        if (Vector2.Distance(transform.position, startPos) < 0.5f)
        {
            transform.position = startPos;
            rb.linearVelocity = Vector2.zero;
            isReturning = false;
            isDashing = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Start returning when hitting anything that's not the player
        if (isDashing && !other.CompareTag("Player"))
        {
            isReturning = true;
            isDashing = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle collision with ground or obstacles
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Obstacle"))
        {
            isReturning = true;
            isDashing = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection range in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
