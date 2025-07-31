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
    public float liftCoefficient = 1.5f;
    public float minLiftSpeed = 0.5f;
    public float maxLiftSpeed = 6f;
    public float liftCurve = 1.2f;
    public Transform liftApplicationPoint;
    public float liftControlSensitivity = 0.8f;

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

        // Apply lift force based on forward velocity
        Vector2 liftForce = CalculateLiftForce();

        // Apply lift at a specific point to allow for realistic aerodynamics
        if (liftApplicationPoint != null)
        {
            rb.AddForceAtPosition(liftForce, liftApplicationPoint.position);
        }
        else
        {
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

    Vector2 CalculateLiftForce()
    {
        // Calculate forward velocity in the direction the plane is facing
        Vector2 forwardDirection = transform.up;
        float forwardVelocity = Vector2.Dot(rb.linearVelocity, forwardDirection);

        // Only apply lift when moving forward and above minimum speed
        if (forwardVelocity < minLiftSpeed)
            return Vector2.zero;

        // Normalize velocity to 0-1 range for lift calculation
        float normalizedVelocity = Mathf.Clamp01((forwardVelocity - minLiftSpeed) / (maxLiftSpeed - minLiftSpeed));

        // Apply curve to make lift feel more natural (stronger at medium speeds)
        float baseLiftStrength = Mathf.Pow(normalizedVelocity, liftCurve) * liftCoefficient;

        // Make lift controllable based on input (pulling up increases lift, pushing down decreases it)
        float liftControl = 1f;
        if (inputDirection.magnitude > 0.1f)
        {
            // Calculate how much the input is pointing "up" relative to the plane
            float upInput = Vector2.Dot(inputDirection, transform.up);
            liftControl = 1f + (upInput * liftControlSensitivity);
        }

        float finalLiftStrength = baseLiftStrength * liftControl;

        // Calculate lift direction (perpendicular to forward direction, upward relative to plane)
        Vector2 liftDirection = new Vector2(-forwardDirection.y, forwardDirection.x);

        // Apply lift force (reduced strength for more controllable feel)
        return liftDirection * finalLiftStrength * forwardVelocity * 0.3f;
    }
}
