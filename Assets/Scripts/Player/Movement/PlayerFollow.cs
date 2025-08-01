using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float speed = 3f;

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition = player.position;
        targetPosition.z = -10f;
        transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
    }
}
