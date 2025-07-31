using UnityEngine;

public class BackAtStart : MonoBehaviour
{
    public static BackAtStart Instance;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && LevelManager.Instance.AllCollectiblesCollected())
        {
            GameObject player = other.gameObject;
            Transform startPoint = LevelManager.Instance.startPoint;
            player.transform.position = new Vector3(startPoint.position.x, startPoint.position.y, 0.1f);
            player.transform.rotation = Quaternion.identity;
            player.GetComponent<PlainController>().started = false;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
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
        Transform startPoint = LevelManager.Instance.startPoint;
        player.transform.position = new Vector3(startPoint.position.x, startPoint.position.y, 0.1f);
        player.transform.rotation = Quaternion.identity;
        player.GetComponent<PlainController>().started = false;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
