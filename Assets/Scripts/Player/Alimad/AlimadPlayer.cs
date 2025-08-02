using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AlimadPlayer : MonoBehaviour
{
    [Header("Lets fly this shit chat")]
    public float speed = 8f;
    public float thrust = 20f;
    public float pitchForce = 2f;
    public float pitchVelocityFactor = 0.2f;
    public float baseDrag = 0.1f;
    public float liftFactor = 1f;
    public float stallSpeed = 4f;
    public float stallHeight = 4f;
    public float rollSpeed = 2f;
    public float highGFactor = 2f;
    public float turbulenceStrength = 1.5f;
    public float turbulenceInterval = 2f;
    public float torqueLimit = 10f;
    private Vector3 offset;
    public GameObject bulletPrefab;
    public Transform barrel;
    public float rst = 0.1f;
    private float timer = 0f;
    public float health = 100f;
    public GameObject body;
    public Camera cam;

    Rigidbody2D rb;
    float roll = 0f;
    float turbulenceTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * speed;
        offset = cam.transform.position - transform.position;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (Input.GetKey(KeyCode.Mouse0) && timer < 0)
        {
            Bullet b = Instantiate(bulletPrefab, barrel.position, barrel.rotation).GetComponent<Bullet>();
            b.initialSpeed = rb.linearVelocity;
            timer = rst;
        }
    }


    void FixedUpdate()
    {
        HandleMovement();
        HandlePitch();
        HandleRoll();
        ApplyLift();
        ApplyTurbulence();
        CheckStall();
    }

    void LateUpdate()
    {
        cam.transform.position = offset + new Vector3(transform.position.x, transform.position.y, 0);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.transform.CompareTag("Ground"))
        {
            Explode();
        }
    }

    void HandleMovement()
    {
        float angleRad = rb.rotation * Mathf.Deg2Rad;
        float pitchY = Mathf.Sin(angleRad);

        if (Input.GetKey(KeyCode.Space))
        {
            speed += thrust * Time.fixedDeltaTime;
        }
        else
        {
            speed *= baseDrag;
        }

        if (pitchY < 0)
        {
            speed += pitchY * -9.8f * Time.fixedDeltaTime;
        }
        Vector2 velocity = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * speed;
        rb.linearVelocity = velocity;
    }

    void HandlePitch()
    {
        float speed = rb.linearVelocity.magnitude;
        float velocityFactor = Mathf.Clamp01(speed * pitchVelocityFactor);
        float multiplier = Input.GetKey(KeyCode.LeftControl) ? highGFactor : 1f;

        if (Input.GetKey(KeyCode.W) && rb.angularVelocity > -(multiplier * torqueLimit)) rb.AddTorque(pitchForce * velocityFactor * multiplier * Time.fixedDeltaTime * Mathf.Cos(roll * Mathf.Deg2Rad));
        if (Input.GetKey(KeyCode.S) && rb.angularVelocity < (multiplier * torqueLimit)) rb.AddTorque(-pitchForce * velocityFactor * multiplier * Time.fixedDeltaTime * Mathf.Cos(roll * Mathf.Deg2Rad));
        rb.angularVelocity *= baseDrag * 0.9f;
    }

    void HandleRoll()
    {
        if (Input.GetKey(KeyCode.A)) roll += rollSpeed;
        if (Input.GetKey(KeyCode.D)) roll -= rollSpeed;

        body.transform.localRotation = Quaternion.Euler(roll, 0f, 0f);
        if (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            Debug.Log(Mathf.Repeat(roll, 180));
        }
    }

    void ApplyLift()
    {
        float speed = rb.linearVelocity.magnitude;
        if (speed > stallSpeed)
        {
            float angle = rb.rotation * Mathf.Deg2Rad;
            Vector2 lift = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)) * liftFactor * Time.fixedDeltaTime;
            rb.AddForce(lift, ForceMode2D.Impulse);
        }
    }

    void ApplyTurbulence()
    {
        turbulenceTimer -= Time.fixedDeltaTime;
        if (turbulenceTimer <= 0f)
        {
            Vector2 turbulence = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * turbulenceStrength;
            rb.AddForce(turbulence, ForceMode2D.Impulse);
            turbulenceTimer = turbulenceInterval + Random.Range(-1f, 1f);
        }
    }

    void CheckStall()
    {
        float speed = rb.linearVelocity.magnitude;
        if (speed < stallSpeed || transform.position.y > stallHeight)
        {
            float angle = Mathf.DeltaAngle(rb.rotation, 0f);
            if (angle > -10f)
                rb.AddTorque(-10f * Time.fixedDeltaTime);
            speed = speed * 0.98f;
        }
    }


    void Explode()
    {
        Debug.Log("Either the plane exploded or ur ass is too lazy to handle a plane");
        Destroy(gameObject);
    }
}
