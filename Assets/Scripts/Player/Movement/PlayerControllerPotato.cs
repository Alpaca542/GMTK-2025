using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControllerPotato : MonoBehaviour
{
    [Header("Movement")]
    public float thrustForce = 10f;
    public float rotationSpeed = 200f;
    public float maxSpeed = 8f;
    [SerializeField] private float sideDrag;
    [SerializeField] private float sideTransfer;
    private float previousRot;

    private Rigidbody2D rb;
    private Vector2 targetInput = Vector2.zero;

    public bool started = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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


    }

    void HandleRotation()
    {

        //my side velocity transfer code is somewhat broken :(
        rb.angularVelocity += -targetInput.x * rotationSpeed;
        float percentRotated = Mathf.Clamp01(Mathf.Abs(rb.rotation - previousRot)/90);
        float badMovement = Vector2.Dot(rb.linearVelocity, transform.right);
        Quaternion rotVec = Quaternion.FromToRotation(Vector3.up, transform.up);
        Debug.DrawRay(transform.position, rotVec * Vector3.right * badMovement);
        rb.linearVelocity -= badMovement * sideTransfer * (Vector2)(rotVec * Vector3.right);
        rb.linearVelocity += badMovement * sideTransfer * (Vector2)(rotVec * Vector3.up);
        badMovement = Vector2.Dot(rb.linearVelocity, transform.right);


        rb.linearVelocity -= badMovement * sideDrag * (Vector2)(rotVec * Vector3.right) * Time.deltaTime;
        previousRot = rb.rotation;

        Debug.DrawRay(transform.position,-( rotVec * Vector3.right * badMovement));

    }

    void HandleMovement()
    {
        rb.AddForce(transform.up * thrustForce);

    }

    void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSpeed) //WELL I NEVER
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}
