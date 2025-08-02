using UnityEngine;

public class Cow : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Magnet"))
        {
            if (other.GetComponent<MagnetScript>().Taken)
            {
                Debug.Log("Cow already taken by magnet.");
                return;
            }
            transform.position = other.transform.position;
            transform.parent = other.transform;
            other.GetComponent<MagnetScript>().Taken = true;
            GameObject.FindAnyObjectByType<PlainController>().OnCowRescuedHandler(gameObject);
        }
        else if (other.CompareTag("Basket"))
        {
            // Reset magnet state
            MagnetScript magnetScript = GameObject.FindAnyObjectByType<MagnetScript>();
            if (magnetScript != null)
            {
                magnetScript.Taken = false;
            }

            // Attach cow to basket properly
            transform.parent = other.transform;
            transform.localPosition = Vector3.zero;

            // Notify PlainController that cow was delivered to basket
            PlainController plainController = GameObject.FindAnyObjectByType<PlainController>();
            if (plainController != null)
            {
                plainController.OnCowDeliveredToBasket();
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RescueCow(gameObject);
            }

            Basket basket = other.GetComponent<Basket>();
            if (basket != null)
            {
                basket.myCows += 1f;
            }
        }
    }
}
