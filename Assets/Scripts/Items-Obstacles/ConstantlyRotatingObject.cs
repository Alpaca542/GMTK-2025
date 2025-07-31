using UnityEngine;

public class ConstantlyRotatingObject : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float speed = 90f;
    private void Update()
    {
        transform.Rotate(rotationAxis, speed * Time.deltaTime, Space.Self);
    }
}
