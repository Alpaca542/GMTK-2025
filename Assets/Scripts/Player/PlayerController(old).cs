using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 150f;

    public bool started = false;

    void Update()
    {
        float rotate = 0f;

        if (!started && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0))
        {
            started = true;
        }

        if (started)
        {
            rotate = -Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
            transform.Translate(Vector3.up * speed * Time.deltaTime);
            transform.Rotate(Vector3.forward * rotate);
        }
    }
}
