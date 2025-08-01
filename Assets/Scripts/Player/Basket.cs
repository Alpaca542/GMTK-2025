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
        cowCountText.text = myCows.ToString();
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

            // Attach basket as child to player
            transform.SetParent(other.transform);
            transform.localPosition = Vector3.zero; // Or adjust to desired offset
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
        return !used && !attachedToPlayer && myCows < minCows;
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
