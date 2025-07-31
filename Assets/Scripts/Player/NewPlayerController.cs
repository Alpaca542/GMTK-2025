using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class NewPlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Velocity Settings")]
    public Transform position;//where we apply force
    public float currentVelocity = 0f;
    public float maxVelocity = 5f;
    public float minVelocity = -5f;
    public float forceCoef = 10f;
    public float accelerationSmoothness = 5f;
    float targetVelocity = 0f;
    public float flapsC = 10f;
    public float decelerationSmoothness = 3f;

    private Rigidbody2D rb;

    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        // float targetVelocity = 0f;

        if (Input.GetKey(KeyCode.D))
        {
            currentVelocity = Mathf.Lerp(currentVelocity, maxVelocity, Time.deltaTime * accelerationSmoothness); ;
        }
        if (Input.GetKey(KeyCode.A))
        {
            currentVelocity = Mathf.Lerp(currentVelocity, 0, Time.deltaTime * accelerationSmoothness); ;
        }


        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;
        Vector2 directionToMouse = mouseWorldPos - transform.position;
        Vector2 targetDirection = -directionToMouse.normalized;

    }
    void FixedUpdate()
    {
        Vector2 force = currentVelocity * forceCoef * (Vector2)transform.right + ApplyLiftForce(currentVelocity);
        rb.AddForce(force, ForceMode2D.Force);

        currentVelocity = Mathf.Lerp(currentVelocity, 0f, Time.deltaTime * decelerationSmoothness);
        rb.AddForceAtPosition(force, position.position, ForceMode2D.Force);
    }

    Vector2 ApplyLiftForce(float motor)
    {

        Vector2 liftForce = (Vector2)transform.up * motor * flapsC;
        return liftForce;
    }
}
