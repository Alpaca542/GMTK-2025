using UnityEngine;
using DG.Tweening;

public class Cow : MonoBehaviour
{
    // void Start()
    // {
    //     // Make the cow oscillate up and down
    //     transform.DOMoveY(transform.position.y + 0.2f, 2f)
    //         .SetEase(Ease.InOutSine)
    //         .SetLoops(-1, LoopType.Yoyo);
    // }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Magnet"))
        {
            MagnetScript magnetScript = other.GetComponent<MagnetScript>();
            if (magnetScript == null)
            {
                Debug.LogError("Magnet does not have MagnetScript component!");
                return;
            }

            if (magnetScript.Taken)
            {
                Debug.Log("Cow already taken by magnet.");
                return;
            }

            chainHolder chainHolderScript = FindAnyObjectByType<chainHolder>();
            if (chainHolderScript == null)
            {
                Debug.LogError("chainHolder not found!");
                return;
            }

            if (!chainHolderScript.isChainDeployed)
            {
                Debug.Log("Chain is not deployed, cannot pick up cow with magnet.");
                return;
            }
            Basket basketScript = GameObject.FindAnyObjectByType<Basket>();
            if (basketScript != null && basketScript.showHint)
            {
                basketScript.SetFirstHint();
            }
            transform.position = other.transform.position;
            transform.parent = other.transform;
            magnetScript.Taken = true;

            PlainController plainController = GameObject.FindAnyObjectByType<PlainController>();
            if (plainController != null)
            {
                plainController.OnCowRescuedHandler(gameObject);
            }
            else
            {
                Debug.LogError("PlainController not found!");
            }
        }
        else if (other.CompareTag("Basket"))
        {
            Debug.Log($"Cow {gameObject.name} triggered by basket {other.name}");

            chainHolder chainHolderScript = FindAnyObjectByType<chainHolder>();
            if (chainHolderScript == null)
            {
                Debug.LogError("chainHolder not found!");
                return;
            }

            if (!chainHolderScript.isChainDeployed)
            {
                Debug.Log("Chain is not deployed, cannot deliver cow to basket.");
                return;
            }

            MagnetScript magnetScript = GameObject.FindAnyObjectByType<MagnetScript>();
            if (magnetScript == null)
            {
                Debug.LogError("MagnetScript not found!");
                return;
            }

            // Check if THIS cow is being carried by the magnet
            bool isCowCarriedByMagnet = (transform.parent != null &&
                                       transform.parent.GetComponent<MagnetScript>() != null);

            if (magnetScript.Taken && !isCowCarriedByMagnet)
            {
                Debug.Log("Magnet is carrying something else, cannot deliver this cow to basket.");
                return;
            }

            // Check if basket can accept more cows
            Basket basket = other.GetComponent<Basket>();
            if (basket == null)
            {
                Debug.LogError("Basket component not found on basket object!");
                return;
            }

            Debug.Log($"Delivering cow {gameObject.name} to basket {other.name}. Basket current cows: {basket.myCows}");

            // Reset magnet state
            magnetScript.Taken = false;

            // Attach cow to basket properly
            transform.parent = other.transform;
            transform.localPosition = Vector3.zero;

            // Notify PlainController that cow was delivered to basket
            PlainController plainController = GameObject.FindAnyObjectByType<PlainController>();
            if (plainController != null)
            {
                plainController.OnCowDeliveredToBasket();
            }

            // Register cow rescue with level manager
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RescueCow(gameObject);
            }

            // Add cow to basket
            basket.PickupCow(gameObject);

            Debug.Log($"Cow delivery complete. Basket now has {basket.myCows} cows");
        }
    }
}
