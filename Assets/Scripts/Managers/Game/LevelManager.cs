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
    [SerializeField] private LevelSwitchAnimation levelSwitchAnimation;
    public bool FirstHalfDone = false;

    // Event for cow rescue
    public static event Action<GameObject> OnCowRescued;


    public void ShowSecondHalf()
    {
        FirstHalfDone = true;
        GameObject.FindAnyObjectByType<LevelEnding>().Activate(true);

        // Store current camera state
        Camera cam = Camera.main;
        PlayerFollow playerFollow = cam.GetComponent<PlayerFollow>();
        CameraZoom cameraZoom = cam.GetComponent<CameraZoom>();

        Vector3 originalPosition = cam.transform.position;
        float originalFOV = cam.fieldOfView;

        // Disable camera components
        playerFollow.enabled = false;
        cameraZoom.enabled = false;

        // Create smooth animation sequence
        Sequence cameraSequence = DOTween.Sequence();

        // Move camera to overview position with smooth easing
        cameraSequence.Append(cam.transform.DOMove(new Vector3(0, -3f, GameObject.FindAnyObjectByType<PlainController>().transform.position.z - 10), 1.5f)
            .SetEase(Ease.InOutQuart));

        // Zoom out with smooth easing
        cameraSequence.Join(cam.DOFieldOfView(88f, 1f)
            .SetEase(Ease.InOutQuart));

        // Hold the overview for a moment
        cameraSequence.AppendInterval(0.5f);

        // Return to original position and FOV
        cameraSequence.Append(cam.transform.DOMove(originalPosition, 1f)
            .SetEase(Ease.InOutQuart));

        cameraSequence.Join(cam.DOFieldOfView(originalFOV, 1f)
            .SetEase(Ease.InOutQuart));

        // Re-enable camera components when animation completes
        cameraSequence.OnComplete(() =>
        {
            playerFollow.enabled = true;
            cameraZoom.enabled = true;
        });
    }

    private void Awake()
    {
        Instance = this;
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        GameObject.FindAnyObjectByType<LevelEnding>().Activate(false);
    }

    public void Start()
    {
        if (currentLevel == 0)
        {
            GameObject.FindAnyObjectByType<CutSceneManager>().StartCutScene();
        }
        // Don't spawn cows immediately - let LevelAddition handle the initial level setup
        // SpawnCows(); // This will be called after the initial level drawing is complete
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

        if (levelSwitchAnimation != null)
        {
            levelSwitchAnimation.AnimateLevelSwitch();
        }
        else
        {
            Debug.LogError("LevelSwitchAnimation is not assigned in the inspector!");
        }
        Invoke(nameof(SwitchFinal), 3f);
    }
    private void SwitchFinal()
    {
        currentLevel++;

        // Reset cow count for new level
        cowCount = 0;
        FirstHalfDone = false;
        GameObject.FindAnyObjectByType<LevelEnding>().Activate(false);

        GameObject player = GameObject.FindAnyObjectByType<PlainController>().gameObject;
        player.GetComponent<PlainController>().isinanim = false;
        player.GetComponent<Rigidbody2D>().gravityScale = player.GetComponent<PlainController>().gravity;

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

        // Notify player controller to reset basket state
        PlainController playerController = player.GetComponent<PlainController>();
        if (playerController != null)
        {
            playerController.OnBasketDelivered(); // This resets carrying state
        }

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

        // Reset player position
        if (startPoint != null)
        {
            player.transform.position = new Vector3(startPoint.position.x, startPoint.position.y, -44.3f);
            player.transform.rotation = Quaternion.identity;

            PlainController controller = player.GetComponent<PlainController>();
            if (controller != null)
            {
                controller.started = false;
                controller.ResetPlayer();
                controller.isdead = false;
            }
        }

        // Call LevelAddition.NextLevel which will handle the drawing animation
        LevelAddition.Instance.NextLevel(currentLevel);

        // Spawn cows after level drawing is complete
        StartCoroutine(WaitForLevelDrawingAndSpawnCows());

        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();
    }

    private IEnumerator WaitForLevelDrawingAndSpawnCows()
    {
        // Wait for the level drawing to complete
        yield return new WaitUntil(() => LevelAddition.Instance != null && !LevelAddition.Instance.IsDrawingLevel);

        // Small delay to ensure everything is properly set up
        yield return new WaitForSeconds(0.1f);

        // Now spawn cows
        SpawnCows();
    }

}
