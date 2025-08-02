using JetBrains.Annotations;
using UnityEngine;
using TMPro;

public class Basket : MonoBehaviour
{
    public bool used = false;
    private bool attachedToPlayer = false;
    public float myCows = 0f;
    public float minCows = 1f;
    public TMP_Text cowCountText;
    public GameObject hint1;
    public GameObject hint2;
    public GameObject hint3;
    public GameObject hint4;
    public bool showHint = false;

    [Header("Cow Textures")]
    public GameObject cowTexture1; // First cow texture
    public GameObject cowTexture2; // Second cow texture  
    public GameObject cowTexture3; // Third cow texture
    public GameObject cowTexture4; // Fourth cow texture
    private float lastCowCount = -1f; // Track changes

    void Update()
    {
        if (cowCountText == null)
        {
            Debug.LogError("Basket: cowCountText is not assigned!");
            return;
        }
        cowCountText.text = myCows.ToString() + " / " + minCows.ToString();
        cowCountText.color = myCows >= minCows ? Color.green : Color.red;

        // Only update textures if cow count changed
        if (lastCowCount != myCows)
        {
            SetCowTexturesVisibility();
            lastCowCount = myCows;
        }
    }
    void Start()
    {
        if (showHint)
        {
            GameObject.FindAnyObjectByType<PlainController>().maxSpeed = 5.5f;
        }

        // Initialize all cow textures as disabled
        SetCowTexturesVisibility();
    }

    private void SetCowTexturesVisibility()
    {
        // Turn off all cow textures first
        if (cowTexture1 != null) cowTexture1.SetActive(false);
        if (cowTexture2 != null) cowTexture2.SetActive(false);
        if (cowTexture3 != null) cowTexture3.SetActive(false);
        if (cowTexture4 != null) cowTexture4.SetActive(false);

        // Turn on textures based on cow count
        if (myCows >= 1 && cowTexture1 != null) cowTexture1.SetActive(true);
        if (myCows >= 2 && cowTexture2 != null) cowTexture2.SetActive(true);
        if (myCows >= 3 && cowTexture3 != null) cowTexture3.SetActive(true);
        if (myCows >= 4 && cowTexture4 != null) cowTexture4.SetActive(true);

        Debug.Log($"Basket: Updated cow textures visibility. Showing {myCows} cow textures");
    }

    public void SetFirstHint()
    {
        hint1.SetActive(showHint);
    }

    public void PickupCow(GameObject cow)
    {
        myCows++;

        // Show hints
        if (myCows == 1f && hint2 != null)
        {
            hint2.SetActive(showHint);
        }
        if (myCows == 2f && hint3 != null)
        {
            hint3.SetActive(showHint);
        }

        // Update cow texture visibility
        SetCowTexturesVisibility();

        // Destroy the actual cow GameObject since we're using textures now
        if (cow != null)
        {
            Debug.Log($"Basket: Collected cow {cow.name}, total cows: {myCows}. Destroying cow GameObject.");
            Destroy(cow);
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
        lastCowCount = -1f; // Reset tracking
        transform.SetParent(null);

        // Reset cow texture visibility
        SetCowTexturesVisibility();

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
            Debug.Log($"Basket ({gameObject.name}): Rigidbody restored to dynamic, cow count and textures reset");
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
