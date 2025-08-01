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
    private float lastInputTime;
    private float inputClearDelay = 0.25f;

    [Header("Lift System")]
    public float liftCoefficient = 2f;
    public float minLiftSpeed = 1f;
    public float maxLiftSpeed = 5f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Vector2 groundCheckOffset = new(0f, -0.5f);
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private FuelManager fuelManager;
    private Vector2 inputDirection;
    private Vector2 targetInput;
    private float currentThrust;
    private bool isGrounded;

    public bool started;
    public float gravity = 4f;
    public bool isdead;
    public bool isinanim;
    public static PlainController Instance;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravity;
        rb.linearDamping = baseDrag;
    }

    void Start()
    {
        fuelManager = FindFirstObjectByType<FuelManager>();
        if (!fuelManager)
            Debug.LogError("FuelManager missing!");
    }

    void FixedUpdate()
    {
        if (!fuelManager || isdead || isinanim || !fuelManager.enabled) return;

        CheckGrounded();
        if (!fuelManager.HasFuel)
        {
            currentThrust = 0f;
            inputDirection = targetInput = Vector2.zero;
            rb.gravityScale = gravity;
            rb.linearDamping = airDrag;
            return;
        }

        ProcessInput();
        RotateToDirection();
        Move();
        ClampVelocity();
        UnstickIfStuck();

        if (started)
            fuelManager.CalculateFuelConsumptionBasedOnThrust(currentThrust);
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle((Vector2)transform.position + groundCheckOffset, groundCheckRadius, groundLayer);
    }

    void ProcessInput()
    {
        Vector2 dir = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) dir += isGrounded ? Vector2.right : Vector2.up;
        if (Input.GetKey(KeyCode.S)) dir += isGrounded ? Vector2.left : Vector2.down;
        if (Input.GetKey(KeyCode.A)) dir += Vector2.left;
        if (Input.GetKey(KeyCode.D)) dir += Vector2.right;

        if (dir != Vector2.zero)
        {
            started = true;
            lastInputTime = Time.time;
            targetInput = dir.normalized;
        }
        else if (Time.time - lastInputTime > inputClearDelay)
        {
            targetInput = Vector2.zero;
        }

        inputDirection = Vector2.Lerp(inputDirection, targetInput, inputSmoothing * Time.fixedDeltaTime);
    }

    void RotateToDirection()
    {
        if (inputDirection.sqrMagnitude < 0.01f) return;

        float targetAngle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg - 90f;
        float angle = rb.rotation;
        float delta = Mathf.DeltaAngle(angle, targetAngle);
        float step = rotationSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(angle + Mathf.Clamp(delta, -step, step));
    }

    void Move()
    {
        if (!started)
        {
            rb.gravityScale = 0f;
            return;
        }

        rb.gravityScale = gravity;
        currentThrust = Mathf.Lerp(currentThrust, targetInput.magnitude, accelerationSmoothing * Time.fixedDeltaTime);

        if (currentThrust > 0.01f)
        {
            Vector2 thrust = currentThrust * thrustForce * transform.up;
            rb.AddForce(thrust);
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
        Vector2 liftDirection = new(-velocity.y, velocity.x);

        float upwardComponent = Vector2.Dot(liftDirection, Vector2.up);
        if (upwardComponent <= 0f) return;

        Vector2 lift = Vector2.up * liftCoefficient * liftStrength * upwardComponent;
        rb.AddForce(lift);
    }

    void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }

    void UnstickIfStuck()
    {
        Vector2 pos = rb.position;
        float offset = 0.4f;
        float nudge = -1.2f;
        float speedLimit = 0.05f;

        bool stuckLeft = Physics2D.OverlapCircle(pos + Vector2.left * offset, 0.1f, groundLayer);
        bool stuckRight = Physics2D.OverlapCircle(pos + Vector2.right * offset, 0.1f, groundLayer);

        bool pressingLeft = Input.GetKey(KeyCode.A);
        bool pressingRight = Input.GetKey(KeyCode.D);

        if (Mathf.Abs(rb.linearVelocity.x) < speedLimit)
        {
            if (stuckLeft && pressingRight)
                rb.linearVelocity += Vector2.right * nudge;
            else if (stuckRight && pressingLeft)
                rb.linearVelocity += Vector2.left * nudge;
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
        inputDirection = targetInput = Vector2.zero;
        currentThrust = 0f;
        started = isdead = isinanim = false;
        fuelManager?.ResetFuel();
    }

    public void AddFuel(float amount)
    {
        fuelManager?.AddFuel(amount);
    }
}
