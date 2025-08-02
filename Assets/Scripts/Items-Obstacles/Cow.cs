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
            GameObject.FindAnyObjectByType<MagnetScript>().Taken = false;
            transform.position = other.transform.position;

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RescueCow(gameObject);
            }
            other.GetComponent<Basket>().myCows += 1f;
        }
    }
}
