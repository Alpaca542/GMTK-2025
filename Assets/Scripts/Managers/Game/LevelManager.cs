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
    [SerializeField] private GameObject halfBorder;

    // Event for cow rescue
    public static event Action<GameObject> OnCowRescued;


    public void ShowSecondHalf()
    {
        FirstHalfDone = true;
        halfBorder.SetActive(false);
        Camera.main.GetComponent<PlayerFollow>().enabled = false;
        Camera.main.GetComponent<CameraZoom>().enabled = false;
        Camera.main.GetComponent<PlayerFollow>().useFirstBounds = false;
        Camera.main.transform.DOMove(new Vector3(0, 0, GameObject.FindAnyObjectByType<PlainController>().transform.position.z - 10), 2f).SetEase(Ease.InOutSine);
        Camera.main.DOFieldOfView(88f, 2f).OnComplete(() =>
        {
            Camera.main.GetComponent<PlayerFollow>().enabled = true;
            Camera.main.GetComponent<CameraZoom>().enabled = true;
        });
    }

    private void Awake()
    {
        Instance = this;
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        halfBorder.SetActive(true);
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
        foreach (var obj in GameObject.FindGameObjectsWithTag("Cow"))
        {
            Destroy(obj);
        }

        activeCows.Clear();

        int spawned = 0;
        int attempts = 0;
        int totalToSpawn = 3 + currentLevel;

        while (spawned < totalToSpawn && attempts < maxAttempts)
        {
            attempts++;
            float x = UnityEngine.Random.Range(topLeftSpawnArea.position.x, bottomRightSpawnArea.position.x);
            float y = UnityEngine.Random.Range(bottomRightSpawnArea.position.y, topLeftSpawnArea.position.y);
            Vector3 spawnPos = new(x, y, -44.3f);
            bool blocked = Physics2D.OverlapCircle((Vector2)spawnPos, 0.3f, spawnBlockingLayers);
            if (!blocked)
            {
                GameObject newCow = Instantiate(cowPrefab, spawnPos, Quaternion.identity);
                newCow.tag = "Cow";
                activeCows.Add(newCow);
                spawned++;
            }
        }

        if (spawned < totalToSpawn)
        {
            Debug.LogWarning($"Only spawned {spawned} of {totalToSpawn} cows. Adjust area or LayerMask.");
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
        halfBorder.SetActive(true);

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
