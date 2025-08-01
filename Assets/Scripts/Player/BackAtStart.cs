using JetBrains.Annotations;
using UnityEngine;

public class BackAtStart : MonoBehaviour
{
    public bool used = false;

    void Start()
    {
        // Reset used flag when the object becomes active (new level starts)
        used = false;
    }

    public void ResetUsed()
    {
        used = false;
        Debug.Log($"BackAtStart ({gameObject.name}): Reset used flag");
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used)
        {
            Debug.Log($"BackAtStart ({gameObject.name}): Already used, ignoring trigger");
            return;
        }
        if (other.CompareTag("Player") && LevelManager.Instance != null && LevelManager.Instance.AllCollectiblesCollected())
        {
            Debug.Log($"BackAtStart ({gameObject.name}): All collectibles collected, proceeding to next level");
            if (!used)
            {
                used = true;
            }
            GameObject player = other.gameObject;
            Transform startPoint = LevelManager.Instance.startPoint;
            if (startPoint == null)
            {
                Debug.LogError("Start point is not assigned in LevelManager!");
                return;
            }

            player.transform.position = new Vector3(startPoint.position.x, startPoint.position.y, -44.3f);
            player.transform.rotation = Quaternion.identity;

            PlainController controller = player.GetComponent<PlainController>();
            if (controller != null)
            {
                controller.started = false;
                controller.ResetPlayer();
            }
            else
            {
                Debug.LogError("PlainController not found on player!");
            }

            LevelManager.Instance.NextLevel();
        }
        else if (other.CompareTag("Player"))
        {
            Debug.Log($"BackAtStart ({gameObject.name}): Collect all collectibles first!");
        }
    }
    public void ResetPlayerPosition(GameObject player)
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager.Instance is null!");
            return;
        }

        Transform startPoint = LevelManager.Instance.startPoint;
        if (startPoint == null)
        {
            Debug.LogError("Start point is not assigned in LevelManager!");
            return;
        }

        player.transform.position = new Vector3(startPoint.position.x, startPoint.position.y, -44.3f);
        player.transform.rotation = Quaternion.identity;

        PlainController controller = player.GetComponent<PlainController>();
        if (controller != null)
        {
            controller.started = false;
            controller.ResetPlayer();
            controller.isdead = false;
        }
        else
        {
            Debug.LogError("PlainController not found on player!");
        }
    }
}
