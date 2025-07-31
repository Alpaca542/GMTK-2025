using UnityEngine;
using UnityEngine.Animations;

public class MovingObject : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;

    private bool movingToB = true;
    void Start()
    {
        if (pointA == null || pointB == null)
        {
            pointA = transform.parent.GetChild(0);
            pointB = transform.parent.GetChild(1);
        }
    }

    private void Update()
    {
        if (pointA == null || pointB == null) return;

        Vector3 target = movingToB ? pointB.position : pointA.position;

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            movingToB = !movingToB;
        }

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }
}
