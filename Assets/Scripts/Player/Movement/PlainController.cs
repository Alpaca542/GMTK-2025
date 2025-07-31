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

    private Rigidbody2D rb;
    private float currentThrust = 0f;
    private float targetThrust = 0f;
    private Vector2 inputDirection = Vector2.zero;
    private Vector2 targetInput = Vector2.zero;
    private FuelManager fuelManager;
    public bool started;
    public float gravity = 4f;
    public bool isdead = false;
    public bool isinanim = false;
    public static PlainController Instance;
    private bool isGrounded = false;
    public LayerMask groundLayer;

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

    void Start()
    {
        fuelManager = FindFirstObjectByType<FuelManager>();
        if (fuelManager == null)
        {
            Debug.LogError("FuelManager not found in the scene.");
        }
    }

    void FixedUpdate()
    {
        if (isdead || isinanim) return;

        if (!fuelManager.HasFuel)
        {
            currentThrust = 0f;
            rb.linearDamping = airDrag;
            rb.gravityScale = gravity;
            return;
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
        if (inputDirection.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

            float maxRotationThisFrame = rotationSpeed * Time.fixedDeltaTime;
            float rotationThisFrame = Mathf.Clamp(angleDiff, -maxRotationThisFrame, maxRotationThisFrame);

            rb.MoveRotation(currentAngle + rotationThisFrame);
        }
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

        // Check if the plane is upside down (z rotation between 90 and 270 degrees)
        float zRotation = transform.eulerAngles.z;
        bool isUpsideDown = false;

        if (upwardComponent > 0)
        {
            Vector2 liftForce = Vector2.up * liftCoefficient * liftStrength * upwardComponent;
            if (isUpsideDown)
            {
                Debug.LogWarning("Plane is upside down, applying negative lift.");
                // Apply a slight negative lift if upside down
                liftForce *= -0.3f;
            }
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
        Invoke(nameof(DelayedReset), 2f);
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
        fuelManager.ResetFuel();
    }

    void DelayedReset()
    {
        BackAtStart.Instance.ResetPlayerPosition(gameObject);
    }
}
