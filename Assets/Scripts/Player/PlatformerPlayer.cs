using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 5f;

    [Header("Jump")]
    public LayerMask groundLayer;
    public Transform groundCheck1;
    public Transform groundCheck2;
    public Transform groundCheck3;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.3f;

    [Header("Wall Sliding")]
    public Transform wallCheckRight;
    public Transform wallCheckLeft;
    public LayerMask wallLayer;
    public float wallSlidingSpeed = 2f;

    [Header("Wall Jumping")]
    public Vector2 wallJumpPowerStrong = new Vector2(8f, 8f);
    public Vector2 wallJumpPowerWeak = new Vector2(4f, 8f);
    public float wallJumpTime = 0.2f;
    public float wallJumpDurationStrong = 0.5f;
    public float wallJumpDurationWeak = 0.15f;

    [Header("Effects")]
    public GameObject bottomParticles;
    public Animator animator;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isFacingRight = true;
    private bool isJumping;
    private bool canRotate = true;
    private bool justGrounded = true;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool jumpPressed;
    private bool jumpReleased;

    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpDirection;
    private float wallJumpCounter;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleInput();
        HandleJumpBuffer();
        HandleJumpRelease();
        HandleJump();
        HandleGrounded();
        HandleWallSlide();
        HandleWallJump();
        HandleFacing();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (!isWallJumping)
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    }

    private void HandleInput()
    {
        moveInput = Input.GetAxis("Horizontal");
        jumpPressed = Input.GetKeyDown(KeyCode.Space);
        jumpReleased = Input.GetKeyUp(KeyCode.Space);
    }

    private void HandleJumpBuffer()
    {
        if (jumpPressed)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleJumpRelease()
    {
        if (jumpReleased && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }
    }

    private void HandleJump()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isJumping)
        {
            jumpBufferCounter = 0f;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJumping = true;
            StartCoroutine(JumpCooldown());
        }
    }

    private void HandleGrounded()
    {
        if (IsGrounded())
        {
            canRotate = true;
            if (!justGrounded)
            {
                justGrounded = true;
                if (bottomParticles) bottomParticles.SetActive(true);
            }
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            justGrounded = false;
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void HandleWallSlide()
    {
        bool walledRight = IsWalled(wallCheckRight);
        bool walledLeft = IsWalled(wallCheckLeft);

        if ((walledRight || walledLeft) && !IsGrounded() && moveInput != 0f && coyoteTimeCounter <= 0f && !isWallJumping)
        {
            canRotate = true;
            if (walledRight)
                Flip();
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void HandleWallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = Mathf.Sign(transform.localScale.x);
            wallJumpCounter = wallJumpTime;
            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpCounter > 0f)
        {
            isWallJumping = true;
            Vector2 jumpPower = Mathf.Sign(moveInput) == Mathf.Sign(transform.localScale.x) ? wallJumpPowerStrong : wallJumpPowerWeak;
            float duration = Mathf.Sign(moveInput) == Mathf.Sign(transform.localScale.x) ? wallJumpDurationStrong : wallJumpDurationWeak;

            rb.linearVelocity = new Vector2(wallJumpDirection * jumpPower.x, jumpPower.y);
            wallJumpCounter = 0f;
            StartCoroutine(RotationCooldown());
            Invoke(nameof(StopWallJumping), duration);
        }
    }

    private void HandleFacing()
    {
        if (!isWallJumping && wallJumpCounter <= 0f && canRotate)
        {
            if ((isFacingRight && moveInput < 0f) || (!isFacingRight && moveInput > 0f))
                Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("stopSTOPsliding", rb.linearVelocity.x);

        if (wallJumpCounter > 0f)
        {
            animator.SetBool("Walking", false);
            animator.SetBool("Jumping", false);
            animator.SetBool("Sliding", true);
        }
        else if (IsGrounded())
        {
            animator.SetBool("Walking", moveInput != 0);
            animator.SetBool("Jumping", false);
            animator.SetBool("Sliding", false);
        }
        else if (isJumping)
        {
            animator.SetBool("Walking", false);
            animator.SetBool("Jumping", true);
            animator.SetBool("Sliding", false);
        }
    }

    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(0.4f);
        isJumping = false;
    }

    private IEnumerator RotationCooldown()
    {
        canRotate = false;
        yield return new WaitForSeconds(0.4f);
        canRotate = true;
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private bool IsGrounded()
    {
        return Physics2D.Raycast(groundCheck1.position, Vector2.down, 0.01f, groundLayer) ||
               Physics2D.Raycast(groundCheck2.position, Vector2.down, 0.01f, groundLayer) ||
               Physics2D.Raycast(groundCheck3.position, Vector2.down, 0.01f, groundLayer);
    }

    private bool IsWalled(Transform wallCheck)
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }
}
