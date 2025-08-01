using UnityEngine;

public class LevelEnding : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Basket"))
        {
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
                }

                // Proceed to next level
                LevelManager.Instance.NextLevel();
            }
        }
    }
}