using UnityEngine;

public class BackAtStart : MonoBehaviour
{
    public static BackAtStart Instance;
    void Awake()
    {
        Instance = this;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && LevelManager.Instance != null && LevelManager.Instance.AllCollectiblesCollected())
        {
            GameObject player = other.gameObject;
            Transform startPoint = LevelManager.Instance.startPoint;
            if (startPoint == null)
            {
                Debug.LogError("Start point is not assigned in LevelManager!");
                return;
            }

            player.transform.position = new Vector3(startPoint.position.x, startPoint.position.y, 0.1f);
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

            Debug.Log("round done");
            LevelManager.Instance.NextLevel();
        }
        else if (other.CompareTag("Player"))
        {
            Debug.Log("Collect all collectibles first!");
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

        player.transform.position = new Vector3(startPoint.position.x, startPoint.position.y, 0.1f);
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
