using UnityEngine;

public class Spike : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("hit");
        if (collision.gameObject.CompareTag("Player"))
        {
            PlainController playerscript = collision.gameObject.GetComponent<PlainController>();
            if (playerscript != null && playerscript.started)
            {
                Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
                playerscript.DieBySpike();
                Debug.Log("Player hit a spike and has been reset.");
            }
            else
            {
                Debug.Log("Player hit a spike but is not started.");
            }
        }
    }
}
