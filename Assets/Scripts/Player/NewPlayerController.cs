using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class NewPlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Velocity Settings")]
    public Transform position;//where we apply force
    public bool accelerate;
    public float forceCoef = 10f;

    float targetVelocity = 0f;
    public float flapsC = 10f;


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
            accelerate = true;

        }
        else
        {
            accelerate = false;
        }



    }
    void FixedUpdate()
    {

        Vector2 force = ApplyLiftForce();
        if (accelerate)
        {
            rb.AddForce((Vector2)transform.right * forceCoef, ForceMode2D.Force);
        }

        rb.AddForceAtPosition(force, position.position, ForceMode2D.Force);
    }

    Vector2 ApplyLiftForce()
    {

        Vector2 liftForce = (Vector2)transform.up * rb.linearVelocity.magnitude * rb.linearVelocity.magnitude * flapsC;
        return liftForce;
    }
}
