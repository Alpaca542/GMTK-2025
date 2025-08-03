using UnityEngine;

public class ConstantlyRotatingObject : MonoBehaviour
{
    [SerializeField] private float speed = 90f;

    private void Update()
    {
        transform.Rotate(0, 0, speed * Time.deltaTime);
    }
}
