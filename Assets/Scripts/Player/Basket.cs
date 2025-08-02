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
    public GameObject hint1;
    public GameObject hint2;
    public GameObject hint3;
    public GameObject hint4;
    public bool showHint = false;
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
    public GameObject cowPrefab;
    public float cowOffsetY = 0.5f;
    void Start()
    {
        if (showHint)
        {
            GameObject.FindAnyObjectByType<PlainController>().maxSpeed = 5.5f;
        }
    }
    public void SetFirstHint()
    {
        hint1.SetActive(showHint);
    }
    public void PickupCow(GameObject cow)
    {
        myCows++;
        if (myCows == 1f && hint2 != null)
        {
            hint2.SetActive(showHint);
        }
        if (myCows == 2f && hint3 != null)
        {
            hint3.SetActive(showHint);
        }

        // Position cow relative to basket
        if (cow != null)
        {
            currentOffset += cowOffsetY;
            // Position cow relative to the basket, not using absolute world position
            cow.transform.localPosition = new Vector3(currentOffset, 0, 0);

            SpriteRenderer sr = cow.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = (int)myCows + 1; // Make sure cows are in front of basket
            }

            Debug.Log($"Basket: Cow {cow.name} positioned at local offset {currentOffset}, total cows: {myCows}");
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
        if (other.CompareTag("Magnet") && !attachedToPlayer)
        {
            Debug.Log($"Basket ({gameObject.name}): Magnet touched basket, attaching and opening second half");

            // Check if magnet already has something
            MagnetScript magnetScript = other.GetComponent<MagnetScript>();
            if (magnetScript != null && magnetScript.Taken)
            {
                Debug.Log($"Basket ({gameObject.name}): Magnet already carrying something, cannot pick up basket");
                return;
            }
            if (showHint)
            {
                hint4.SetActive(true);
            }
            // Disable rigidbody physics before attaching
            Rigidbody2D basketRb = GetComponent<Rigidbody2D>();
            if (basketRb != null)
            {
                basketRb.bodyType = RigidbodyType2D.Kinematic;
                basketRb.linearVelocity = Vector2.zero;
                basketRb.angularVelocity = 0f;
                Debug.Log($"Basket ({gameObject.name}): Rigidbody set to kinematic for carrying");
            }

            // Attach to magnet
            transform.position = other.transform.position;
            transform.parent = other.transform;

            // Set magnet as taken
            if (magnetScript != null)
            {
                magnetScript.Taken = true;
            }

            // Notify PlainController that basket is picked up
            PlainController playerController = GameObject.FindAnyObjectByType<PlainController>();
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
        myCows = 0f;
        currentOffset = 0f;
        transform.SetParent(null);

        // Reset magnet state when basket is reset
        MagnetScript magnetScript = GameObject.FindAnyObjectByType<MagnetScript>();
        if (magnetScript != null)
        {
            magnetScript.Taken = false;
        }

        // Restore rigidbody physics when reset
        Rigidbody2D basketRb = GetComponent<Rigidbody2D>();
        if (basketRb != null)
        {
            basketRb.bodyType = RigidbodyType2D.Dynamic;
            Debug.Log($"Basket ({gameObject.name}): Rigidbody restored to dynamic, cow count reset");
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
