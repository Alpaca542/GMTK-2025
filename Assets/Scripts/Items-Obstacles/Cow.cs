using UnityEngine;

public class Cow : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Magnet"))
        {
            // Move cow to magnet position
            transform.position = other.transform.position;
            transform.parent = other.transform;
            other.GetComponent<MagnetScript>().Taken = true;
        }
        else if (other.CompareTag("Basket"))
        {
            GameObject.FindAnyObjectByType<MagnetScript>().Taken = false;
            transform.position = other.transform.position;

            // Remove from active cows and add to cow count
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RescueCow(gameObject);
            }
            other.GetComponent<Basket>().myCows += 1f;
        }
    }
}
