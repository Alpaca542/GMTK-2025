using UnityEngine;

public class BackAtStart : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && LevelManager.Instance.AllCollectiblesCollected())
        {
            GameObject player = other.gameObject;
            Transform startPoint = LevelManager.Instance.startPoint;
            player.transform.position = new Vector3(startPoint.position.x, startPoint.position.y, 0.1f);
            player.transform.rotation = Quaternion.identity;
            PlaneController planeController = player.GetComponent<PlaneController>();
            planeController.started = false;
            Debug.Log("round done");
            LevelManager.Instance.NextLevel();
        }
        else if (other.CompareTag("Player"))
        {
            Debug.Log("Collect all collectibles first!");
        }
    }
}
