using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float speed = 3f;

    public bool useFirstBounds = true;
    [SerializeField] private Transform minBounds1;
    [SerializeField] private Transform maxBounds1;
    [SerializeField] private Transform minBounds2;
    [SerializeField] private Transform maxBounds2;

    private bool isLevelTransitioning = false;

    private void LateUpdate()
    {
        if (player == null) return;

        // Don't follow during level transitions
        if (isLevelTransitioning || (LevelAddition.Instance != null && LevelAddition.Instance.IsDrawingLevel))
        {
            return;
        }

        Vector3 targetPosition = player.position;
        targetPosition.z = player.transform.position.z - 10f;

        // Select bounds based on the boolean
        Transform minBounds = useFirstBounds ? minBounds1 : minBounds2;
        Transform maxBounds = useFirstBounds ? maxBounds1 : maxBounds2;

        // Clamp the target position within the bounds
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.position.x, maxBounds.position.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.position.y, maxBounds.position.y);

        transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
    }

    // Method to be called by LevelAddition to prevent interference
    public void SetTransitionMode(bool transitioning)
    {
        isLevelTransitioning = transitioning;
    }
}
