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
        else if (other.CompareTag("Basket") && GameObject.FindAnyObjectByType<chainHolder>().isChainDeployed == true)
        {
            if (GameObject.FindAnyObjectByType<MagnetScript>().Taken)
            {
                Debug.Log("Cow cannot be picked up by basket while magnet is active.");
                return;
            }

            if (GameObject.FindAnyObjectByType<Basket>().showHint)
            {
                GameObject.FindAnyObjectByType<Basket>().SetFirstHint();
            }

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
            basket.PickupCow(gameObject);
        }
    }
}
