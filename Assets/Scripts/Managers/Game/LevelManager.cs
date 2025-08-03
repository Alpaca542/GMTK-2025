using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LayerMask spawnBlockingLayers;
    [SerializeField] private int maxAttempts = 100;
    public Transform topLeftSpawnArea;
    public Transform bottomRightSpawnArea;
    public static LevelManager Instance;
    public Transform startPoint;
    public GameObject cowPrefab;
    public int currentLevel = 0;
    private List<GameObject> activeCows = new();
    private int cowCount = 0;
    public bool FirstHalfDone = false;
    public GameObject oldPlane;
    public float zoomedOutFOV;
    public Transform zoomedOutPosition;
    public GameObject player;

    // Event for cow rescue
    public static event Action<GameObject> OnCowRescued;


    public void ShowSecondHalf()
    {
        FirstHalfDone = true;
        GameObject.FindAnyObjectByType<LevelEnding>().Activate(true);
    }

    private void Awake()
    {
        Instance = this;
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        GameObject.FindAnyObjectByType<LevelEnding>().Activate(false);
    }

    public void SpawnIn()
    {
        SpawnCows();
    }

    #region Cows
    public bool AllCowsRescued()
    {
        return activeCows.Count == 0;
    }

    public void RescueCow(GameObject cow)
    {
        activeCows.Remove(cow);
        cowCount++;

        // Trigger the event before destroying the cow
        OnCowRescued?.Invoke(cow);

        Destroy(cow);
    }

    public int GetCowCount()
    {
        return cowCount;
    }

    private void SpawnCows()
    {
        activeCows.Clear();

        GameObject[] existingCows = GameObject.FindGameObjectsWithTag("Cow");
        foreach (GameObject cow in existingCows)
        {
            activeCows.Add(cow);
        }
    }


    #endregion
    public void NextLevel()
    {
        if (!FirstHalfDone)
        {
            ShowSecondHalf();
            return;
        }

        Invoke(nameof(SwitchFinal), 0.5f);
    }
    private void SwitchFinal()
    {
        Debug.Log($"Starting level transition from {currentLevel} to {currentLevel + 1}");

        currentLevel++;

        // Reset cow count for new level
        cowCount = 0;
        FirstHalfDone = false;
        GameObject.FindAnyObjectByType<LevelEnding>().Activate(false);

        // // Ensure player is ready for transition
        // player.SetActive(true);
        // PlainController playerController = player.GetComponent<PlainController>();
        // if (playerController != null)
        // {
        //     playerController.isinanim = true; // Keep player paused during transition
        //     playerController.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        //     playerController.GetComponent<Rigidbody2D>().angularVelocity = 0f;
        // }

        // Reset and delete all baskets
        Basket[] baskets = GameObject.FindObjectsByType<Basket>(FindObjectsSortMode.None);
        foreach (Basket basket in baskets)
        {
            if (basket.transform.parent != null)
            {
                basket.transform.SetParent(null);
            }
            Destroy(basket.gameObject);
        }

        // // Notify player controller to reset basket state
        // if (playerController != null)
        // {
        //     playerController.OnBasketDelivered(); // This resets carrying state
        // }

        // Clean up any remaining cows from previous level
        GameObject[] remainingCows = GameObject.FindGameObjectsWithTag("Cow");
        foreach (GameObject cow in remainingCows)
        {
            Destroy(cow);
        }
        activeCows.Clear();

        // Reset any magnet scripts
        MagnetScript[] magnets = GameObject.FindObjectsByType<MagnetScript>(FindObjectsSortMode.None);
        foreach (MagnetScript magnet in magnets)
        {
            magnet.Taken = false;
        }

        // Create the old plane effect
        GameObject dead = Instantiate(oldPlane, player.transform.position, player.transform.rotation);
        dead.GetComponent<FadeOut>().FadeMeOut();
        // Reset player position
        // if (startPoint != null)
        // {
        //     player.transform.position = new Vector3(startPoint.position.x, startPoint.position.y, -44.3f);
        //     player.transform.rotation = Quaternion.identity;

        //     if (playerController != null)
        //     {
        //         playerController.started = false;
        //         playerController.ResetPlayer();
        //         playerController.isdead = false;
        //         // Keep isinanim true during transition - will be reset by LevelAddition
        //     }
        // }
        // player.SetActive(false);
        // Save progress
        player.GetComponent<PlainController>().CleanUp();
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();

        // Start the level transition animation
        // LevelAddition.NextLevel will handle the full animation sequence
        if (LevelAddition.Instance != null)
        {
            LevelAddition.Instance.NextLevel(currentLevel, player);
        }
        else
        {
            Debug.LogError("LevelAddition.Instance is null!");
        }

        // Spawn cows after level drawing is complete
        StartCoroutine(WaitForLevelDrawingAndSpawnCows());
    }

    private IEnumerator WaitForLevelDrawingAndSpawnCows()
    {
        // Wait for the level drawing to complete
        yield return new WaitUntil(() => LevelAddition.Instance != null && !LevelAddition.Instance.IsDrawingLevel);

        // Spawn cows immediately - no unnecessary delay
        SpawnCows();
    }

}
