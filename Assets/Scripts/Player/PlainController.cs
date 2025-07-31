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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = baseDrag;
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
        HandleInput();
        HandleRotation();
        HandleMovement();
        ClampVelocity();
        fuelManager.CalculateFuelConsumptionBasedOnThrust(currentThrust);
    }

    void HandleInput()
    {
        targetInput = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) targetInput += Vector2.up;
        if (Input.GetKey(KeyCode.S)) targetInput += Vector2.down;
        if (Input.GetKey(KeyCode.A)) targetInput += Vector2.left;
        if (Input.GetKey(KeyCode.D)) targetInput += Vector2.right;
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

            transform.rotation = Quaternion.Euler(0, 0, currentAngle + rotationThisFrame);
        }
    }

    void HandleMovement()
    {
        float inputMagnitude = inputDirection.magnitude;
        targetThrust = Mathf.Clamp01(inputMagnitude);
        currentThrust = Mathf.Lerp(currentThrust, targetThrust, accelerationSmoothing * Time.fixedDeltaTime);

        // Apply thrust
        if (currentThrust > 0.01f)
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

        // Apply lift force
        ApplyLift();
    }

    void ApplyLift()
    {
        float speed = rb.linearVelocity.magnitude;
        if (speed < minLiftSpeed) return;
        float liftStrength = Mathf.Clamp01(speed / maxLiftSpeed);
        Vector2 velocity = rb.linearVelocity.normalized;
        Vector2 liftDirection = new Vector2(-velocity.y, velocity.x); // Perpendicular to velocity

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
}
