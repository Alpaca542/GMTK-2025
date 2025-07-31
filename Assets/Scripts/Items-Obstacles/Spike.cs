using UnityEngine;

public class Spike : MonoBehaviour
{
    void OTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
           PlainController playerscript = collision.GetComponent<PlainController>();
            if (playerscript != null && playerscript.started)
            {
                Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
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
