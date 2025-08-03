using UnityEngine;

public class LevelEnding : MonoBehaviour
{
    public bool active = false;
    public GameObject mySign;
    public void Activate(bool isActive)
    {
        active = isActive;
        if (mySign != null)
        {
            mySign.SetActive(isActive);
        }
        GetComponent<SpriteRenderer>().enabled = isActive;
    }
    void Start()
    {
        if (mySign != null)
        {
            mySign.SetActive(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Basket") && active)
        {
            active = false;
            Debug.Log("Basket reached the end point!");

            if (LevelManager.Instance != null)
            {
                int cowsRescued = LevelManager.Instance.GetCowCount();
                Debug.Log($"Level completed! Cows rescued: {cowsRescued}");

                // Mark the basket as used to prevent further interactions
                Basket basket = other.GetComponent<Basket>();
                if (basket != null)
                {
                    basket.used = true;

                    // Notify player controller that basket is delivered
                    PlainController playerController = FindFirstObjectByType<PlainController>();
                    if (playerController != null)
                    {
                        playerController.OnBasketDelivered();
                    }
                    playerController.GetComponent<Rigidbody2D>().AddForce(transform.position - playerController.transform.position * 10f, ForceMode2D.Impulse);
                }

                // Proceed to next level
                LevelManager.Instance.NextLevel();
            }
        }
    }
}