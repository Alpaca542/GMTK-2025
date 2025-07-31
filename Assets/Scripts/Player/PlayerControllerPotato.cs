using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControllerPotato : MonoBehaviour
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
    [SerializeField] private float sideDrag;
    [SerializeField] private float sideTransfer;
    private float previousRot;

    private Rigidbody2D rb;
    private float currentThrust = 0f;
    private float targetThrust = 0f;
    private Vector2 inputDirection = Vector2.zero;
    private Vector2 targetInput = Vector2.zero;

    public bool started = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = baseDrag;
    }
    void Update()
    {
        if (!started && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0))
        {
            started = true;
        }
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
        targetInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        /*
        targetInput = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) targetInput += Vector2.up;
        if (Input.GetKey(KeyCode.S)) targetInput += Vector2.down;
        if (Input.GetKey(KeyCode.A)) targetInput += Vector2.left;
        if (Input.GetKey(KeyCode.D)) targetInput += Vector2.right;
        inputDirection = Vector2.Lerp(inputDirection, targetInput.normalized, inputSmoothing * Time.fixedDeltaTime);
        *///smoothing input is for babies!!!
    }

    void HandleRotation()
    {
        /*if (inputDirection.magnitude > 0.2f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
            float rotationAmount = Mathf.Sign(angleDiff) * Mathf.Min(Mathf.Abs(angleDiff), rotationSpeed * Time.fixedDeltaTime);
            float smoothedRotation = Mathf.Lerp(0, rotationAmount, additionalRotationSmoothing * Time.fixedDeltaTime);
            transform.Rotate(0, 0, smoothedRotation);
        }*/

        rb.angularVelocity += -targetInput.x * rotationSpeed;
        float percentRotated = Mathf.Clamp01(Mathf.Abs(rb.rotation - previousRot)/90);
        float badMovement = Vector2.Dot(rb.linearVelocity, transform.right);
        Quaternion rotVec = Quaternion.FromToRotation(Vector3.up, transform.up);
        rb.linearVelocity -= badMovement * sideTransfer * (Vector2)(rotVec * Vector3.right) * percentRotated;
        rb.linearVelocity += badMovement * sideTransfer * (Vector2)(rotVec * Vector3.up) * percentRotated;
        badMovement = Vector2.Dot(rb.linearVelocity, transform.right);
        rb.linearVelocity -= badMovement * sideDrag * (Vector2)(rotVec * Vector3.right) * Time.deltaTime;
        previousRot = rb.rotation;
    }

    void HandleMovement()
    {
        rb.AddForce(transform.up * thrustForce);
        /*float inputMagnitude = inputDirection.magnitude;
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
        }*/
    }

    void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSpeed) //WELL I NEVER
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}
