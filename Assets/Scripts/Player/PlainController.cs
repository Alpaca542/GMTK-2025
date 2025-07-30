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

    private Rigidbody2D rb;
    private float currentThrust = 0f;
    private float targetThrust = 0f;
    private Vector2 inputDirection = Vector2.zero;
    private Vector2 targetInput = Vector2.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = baseDrag;
    }

    void FixedUpdate()
    {
        HandleInput();
        HandleRotation();
        HandleMovement();
        ClampVelocity();
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
        if (inputDirection.magnitude > 0.2f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
            float rotationAmount = Mathf.Sign(angleDiff) * Mathf.Min(Mathf.Abs(angleDiff), rotationSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle), additionalRotationSmoothing * Time.fixedDeltaTime);
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
    }

    void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}
