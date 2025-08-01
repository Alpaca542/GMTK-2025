using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AlimadPlayer : MonoBehaviour
{
    [Header("Flight Settings")]
    public float thrust = 20f;
    public float pitchForce = 2f;
    public float pitchVelocityFactor = 0.2f;
    public float baseDrag = 0.1f;
    public float liftFactor = 1f;
    public float takeoffSpeed = 6f;
    public float stallHeight = 40f;
    public float rollSpeed = 2f;
    public float highGFactor = 2f;

    public GameObject body;
    public Camera cam;

    Rigidbody2D rb;
    float targetRoll = 0f;
    bool isGrounded = true;
    bool rolling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandlePitch();
        HandleRoll();
        ApplyLift();
        ApplyDrag();
        CheckStall();
    }

    void LateUpdate()
    {
        cam.transform.position = new Vector3(transform.position.x, transform.position.y, cam.transform.position.z);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        isGrounded = false;
    }

    void HandleMovement()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            float angle = rb.rotation * Mathf.Deg2Rad;
            Vector2 force = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * thrust;
            rb.AddForce(force * Time.fixedDeltaTime, ForceMode2D.Impulse);
        }
    }

    void HandlePitch()
    {
        float speed = rb.linearVelocity.magnitude;
        float velocityFactor = Mathf.Clamp01(speed * pitchVelocityFactor);
        float multiplier = Input.GetKey(KeyCode.LeftControl) ? highGFactor : 1f;

        if (Input.GetKey(KeyCode.W)) rb.AddTorque(pitchForce * velocityFactor * multiplier * Time.fixedDeltaTime);
        if (Input.GetKey(KeyCode.S)) rb.AddTorque(-pitchForce * velocityFactor * multiplier * Time.fixedDeltaTime);
    }

    void HandleRoll()
    {
        if (!isGrounded && !rolling)
        {
            if (Input.GetKeyDown(KeyCode.A)) { targetRoll += 180f; rolling = true; }
            if (Input.GetKeyDown(KeyCode.D)) { targetRoll -= 180f; rolling = true; }
        }

        float currentX = Mathf.Repeat(body.transform.localEulerAngles.x, 360f);
        float rollTarget = Mathf.Repeat(targetRoll, 360f);

        currentX = Mathf.MoveTowardsAngle(currentX, rollTarget, rollSpeed);
        body.transform.localRotation = Quaternion.Euler(currentX, 0f, 0f);

        if (Mathf.Abs(Mathf.DeltaAngle(currentX, rollTarget)) < 1f)
            rolling = false;
    }

    void ApplyLift()
    {
        float speed = rb.linearVelocity.magnitude;
        if (!isGrounded && speed > takeoffSpeed)
        {
            float angle = rb.rotation * Mathf.Deg2Rad;
            Vector2 lift = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)) * liftFactor * Time.fixedDeltaTime;
            rb.AddForce(lift, ForceMode2D.Impulse);
        }
    }

    void ApplyDrag()
    {
        Vector2 dragForce = -rb.linearVelocity.normalized * baseDrag;
        rb.AddForce(dragForce, ForceMode2D.Force);
    }

    void CheckStall()
    {
        if (transform.position.y > stallHeight)
        {
            rb.AddTorque(Random.Range(-20f, 20f) * Time.fixedDeltaTime);
            rb.AddForce(Vector2.down * 3f, ForceMode2D.Force);
        }
    }
}
