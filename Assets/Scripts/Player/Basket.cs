using JetBrains.Annotations;
using UnityEngine;
using TMPro;

public class Basket : MonoBehaviour
{
    public bool used = false;
    private bool attachedToPlayer = false;
    public float myCows = 0f;
    public float minCows = 1f;
    public float currentOffset = 0f;
    public TMP_Text cowCountText;
    void Update()
    {
        if (cowCountText == null)
        {
            Debug.LogError("Basket: cowCountText is not assigned!");
            return;
        }
        cowCountText.text = myCows.ToString() + " / " + minCows.ToString();
        cowCountText.color = myCows >= minCows ? Color.green : Color.red;
    }
    public GameObject cowPrefab; // Assign in inspector
    public float cowOffsetY = 0.5f; // Vertical offset between cows

    public void PickupCow(GameObject cow)
    {
        myCows++;

        if (cowPrefab != null)
        {
            currentOffset += cowOffsetY;
            cow.transform.position = new Vector3(transform.position.x + currentOffset, transform.position.y, transform.position.z);
            SpriteRenderer sr = cow.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = (int)myCows;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPickable())
        {
            Debug.Log($"Basket ({gameObject.name}): Already used, ignoring trigger");
            return;
        }

        // When player touches basket, attach it to the player and open second half
        if (other.CompareTag("Player") && !attachedToPlayer)
        {
            Debug.Log($"Basket ({gameObject.name}): Player touched basket, attaching and opening second half");

            // Disable rigidbody physics before attaching
            Rigidbody2D basketRb = GetComponent<Rigidbody2D>();
            if (basketRb != null)
            {
                basketRb.bodyType = RigidbodyType2D.Kinematic;
                Debug.Log($"Basket ({gameObject.name}): Rigidbody set to kinematic for carrying");
            }

            // Notify PlainController that basket is picked up
            PlainController playerController = other.GetComponent<PlainController>();
            if (playerController != null)
            {
                playerController.OnBasketPickedUp(transform);
            }

            attachedToPlayer = true;

            // Open the way to second half
            if (LevelManager.Instance != null && !LevelManager.Instance.FirstHalfDone)
            {
                LevelManager.Instance.ShowSecondHalf();
            }

            return;
        }
    }

    public bool IsPickable()
    {
        return !used && !attachedToPlayer && myCows >= minCows;
    }

    public void ResetPlayerPosition(GameObject player)
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager.Instance is null!");
            return;
        }

        Transform startPoint = LevelManager.Instance.startPoint;
        if (startPoint == null)
        {
            Debug.LogError("Start point is not assigned in LevelManager!");
            return;
        }

        player.transform.position = new Vector3(startPoint.position.x, startPoint.position.y, -44.3f);
        player.transform.rotation = Quaternion.identity;

        PlainController controller = player.GetComponent<PlainController>();
        if (controller != null)
        {
            controller.started = false;
            controller.ResetPlayer();
            controller.isdead = false;
        }
        else
        {
            Debug.LogError("PlainController not found on player!");
        }
    }

    public void ResetBasket()
    {
        used = false;
        attachedToPlayer = false;
        transform.SetParent(null);

        // Restore rigidbody physics when reset
        Rigidbody2D basketRb = GetComponent<Rigidbody2D>();
        if (basketRb != null)
        {
            basketRb.bodyType = RigidbodyType2D.Dynamic;
            Debug.Log($"Basket ({gameObject.name}): Rigidbody restored to dynamic");
        }
    }

    public void DestroyBasket()
    {
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        Destroy(gameObject);
    }
}
