using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlainController : MonoBehaviour
{
    [Header("Movement")]
    public float thrustForce = 10f;
    public float rotationSpeed = 200f;
    public float maxSpeed = 8f;
    public float baseDrag = 1f;
    public float airDrag = 3f;
    public float inputSmoothing = 8f;
    public float accelerationSmoothing = 12f;
    public float momentumDrag = 0.98f;
    public float additionalRotationSmoothing = 0.5f;

    [Header("Lift System")]
    public float liftCoefficient = 2f;
    public float minLiftSpeed = 1.0f;
    public float maxLiftSpeed = 5f;

    [Header("Wall Avoidance")]
    public float wallDetectionDistance = 1.5f;
    public float avoidanceRotationSpeed = 300f;
    public float avoidanceDuration = 0.8f;
    public float avoidanceThrust = 0.7f;
    public LayerMask wallLayer = -1;

    private Rigidbody2D rb;
    private float currentThrust = 0f;
    private float targetThrust = 0f;
    private Vector2 inputDirection = Vector2.zero;
    private Vector2 targetInput = Vector2.zero;
    [SerializeField] private FuelManager fuelManager;
    public bool started;
    public float gravity = 4f;
    public bool isdead = false;
    public bool isinanim = false;
    public static PlainController Instance;
    private bool isGrounded = false;
    public LayerMask groundLayer;

    // Wall avoidance variables
    private bool isAvoidingWall = false;
    private float avoidanceTimer = 0f;
    private float targetAvoidanceAngle = 0f;

    public void AddFuel(float amount)
    {
        if (fuelManager != null)
        {
            fuelManager.AddFuel(amount);
        }
        else
        {
            Debug.LogError("FuelManager is not assigned in PlainController.");
        }
    }

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = baseDrag;
        gravity = rb.gravityScale;
    }

    void isGroundedCheck()
    {
        float checkRadius = 0.15f;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, checkRadius, groundLayer);
        isGrounded = hit != null;
    }

    bool DetectWallInFront()
    {
        Vector2 forwardDirection = transform.up;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, forwardDirection, wallDetectionDistance, wallLayer);

        // Debug visualization (optional - remove in production)
        Debug.DrawRay(transform.position, forwardDirection * wallDetectionDistance, hit.collider != null ? Color.red : Color.green);

        return hit.collider != null;
    }

    void StartWallAvoidance()
    {
        isAvoidingWall = true;
        avoidanceTimer = avoidanceDuration;

        // Always turn 150 degrees from current direction
        float currentAngle = transform.eulerAngles.z;
        targetAvoidanceAngle = currentAngle + 120f;

        // Normalize angle
        while (targetAvoidanceAngle >= 180f) targetAvoidanceAngle -= 360f;
        while (targetAvoidanceAngle < -180f) targetAvoidanceAngle += 360f;
    }

    void Start()
    {
        if (!fuelManager)
        {
            fuelManager = FindFirstObjectByType<FuelManager>();
        }
        if (fuelManager == null)
        {
            Debug.LogError("Someone deleted the FeulManager :skulk:");
        }
    }

    void FixedUpdate()
    {
        if (fuelManager.enabled == false)
        {
            Debug.LogWarning("FuelManager is disabled, hmmmm someone might've been sabotaging.");
            return;
        }
        if (isdead || isinanim) return;

        if (!fuelManager.HasFuel)
        {
            currentThrust = 0f;
            rb.linearDamping = airDrag;
            rb.gravityScale = gravity;
            return;
        }

        // Wall avoidance timer update
        if (isAvoidingWall)
        {
            avoidanceTimer -= Time.fixedDeltaTime;
            if (avoidanceTimer <= 0f)
            {
                isAvoidingWall = false;
                // Reset input direction immediately to prevent auto-looking upward
                inputDirection = Vector2.zero;
                targetInput = Vector2.zero;
            }
        }

        // Check for walls and start avoidance if needed
        if (!isAvoidingWall && started && DetectWallInFront())
        {
            StartWallAvoidance();
        }

        HandleInput();
        HandleRotation();
        HandleMovement();
        ClampVelocity();
        if (started) fuelManager.CalculateFuelConsumptionBasedOnThrust(currentThrust);
    }


    void HandleInput()
    {
        targetInput = Vector2.zero;

        bool hasInput = false;

        isGroundedCheck();

        // Block player input during wall avoidance
        if (isAvoidingWall)
        {
            // During avoidance, maintain some input to keep thrust active
            targetInput = Vector2.up * avoidanceThrust;
            inputDirection = Vector2.Lerp(inputDirection, targetInput.normalized, inputSmoothing * Time.fixedDeltaTime);
            return;
        }

        if (isGrounded)
        {
            if (Input.GetKey(KeyCode.W)) { targetInput += Vector2.right; hasInput = true; }
            if (Input.GetKey(KeyCode.S)) { targetInput += Vector2.left; hasInput = true; }
            if (Input.GetKey(KeyCode.A)) { targetInput += Vector2.left; hasInput = true; }
            if (Input.GetKey(KeyCode.D)) { targetInput += Vector2.right; hasInput = true; }
        }
        else
        {
            if (Input.GetKey(KeyCode.W)) { targetInput += Vector2.up; hasInput = true; }
            if (Input.GetKey(KeyCode.S)) { targetInput += Vector2.down; hasInput = true; }
            if (Input.GetKey(KeyCode.A)) { targetInput += Vector2.left; hasInput = true; }
            if (Input.GetKey(KeyCode.D)) { targetInput += Vector2.right; hasInput = true; }
        }

        if (hasInput)
        {
            started = true;
        }

        inputDirection = Vector2.Lerp(inputDirection, targetInput.normalized, inputSmoothing * Time.fixedDeltaTime);
    }


    void HandleRotation()
    {
        float currentAngle = transform.eulerAngles.z;
        float targetAngle;
        float rotSpeed;

        if (isAvoidingWall)
        {
            // During wall avoidance, rotate to target avoidance angle
            targetAngle = targetAvoidanceAngle;
            rotSpeed = avoidanceRotationSpeed;
        }
        else if (inputDirection.magnitude > 0.1f)
        {
            // Normal player-controlled rotation
            targetAngle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg - 90f;
            rotSpeed = rotationSpeed;
        }
        else
        {
            return; // No rotation needed
        }

        float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
        float maxRotationThisFrame = rotSpeed * Time.fixedDeltaTime;
        float rotationThisFrame = Mathf.Clamp(angleDiff, -maxRotationThisFrame, maxRotationThisFrame);

        rb.MoveRotation(currentAngle + rotationThisFrame);
    }

    void HandleMovement()
    {
        if (!started)
        {
            rb.gravityScale = 0f;
            return;
        }

        rb.gravityScale = gravity;

        float inputMagnitude = inputDirection.magnitude;
        targetThrust = Mathf.Clamp01(inputMagnitude);
        currentThrust = Mathf.Lerp(currentThrust, targetThrust, accelerationSmoothing * Time.fixedDeltaTime);

        if (currentThrust > 0.01f && fuelManager.HasFuel)
        {
            Vector2 thrustDirection = transform.up;
            rb.AddForce(thrustForce * currentThrust * thrustDirection);
            rb.linearDamping = baseDrag;
        }
        else
        {
            rb.linearDamping = airDrag;
            rb.linearVelocity *= momentumDrag;
        }

        ApplyLift();
    }


    void ApplyLift()
    {
        float speed = rb.linearVelocity.magnitude;
        if (speed < minLiftSpeed) return;
        float liftStrength = Mathf.Clamp01(speed / maxLiftSpeed);
        Vector2 velocity = rb.linearVelocity.normalized;
        Vector2 liftDirection = new(-velocity.y, velocity.x); // Perpendicular to velocity

        // Only apply upward component of lift to prevent strange behavior
        float upwardComponent = Vector2.Dot(liftDirection, Vector2.up);

        if (upwardComponent > 0)
        {
            Vector2 liftForce = Vector2.up * liftCoefficient * liftStrength * upwardComponent;
            rb.AddForce(liftForce);
        }
    }

    void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
    public void DieBySpike()
    {
        Debug.Log("Player has died by spike.");
        isdead = true;
        rb.gravityScale = 0f;
        started = false;
    }
    public void ResetPlayer()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.rotation = 0f;
        rb.linearDamping = baseDrag;
        inputDirection = Vector2.zero;
        targetInput = Vector2.zero;
        currentThrust = 0f;
        started = false;
        isdead = false;
        isinanim = false;

        // Reset wall avoidance state
        isAvoidingWall = false;
        avoidanceTimer = 0f;
        targetAvoidanceAngle = 0f;

        fuelManager.ResetFuel();
    }

    // void DelayedReset()
    // {
    //     BackAtStart.Instance.ResetPlayerPosition(gameObject);
    // }
}
