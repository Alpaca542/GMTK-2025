using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
public class LevelManager : MonoBehaviour
{
    [SerializeField] private LayerMask spawnBlockingLayers;
    [SerializeField] private int maxAttempts = 100;
    public Transform topLeftSpawnArea;
    public Transform bottomRightSpawnArea;
    public static LevelManager Instance;
    public Transform startPoint;
    public GameObject collectiblePrefab;
    public int currentLevel = 0;
    private List<GameObject> activeCollectibles = new();
    [SerializeField] private LevelSwitchAnimation levelSwitchAnimation;
    public bool FirstHalfDone = false;
    [SerializeField] private GameObject halfBorder;
    [SerializeField] private GameObject backPos1;
    [SerializeField] private GameObject backPos2;


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

        // Safely handle BackPos fade animations
        if (backPos1 != null && backPos1.GetComponent<SpriteRenderer>() != null)
        {
            backPos1.GetComponent<SpriteRenderer>().DOFade(0f, 2f);
        }
        else
        {
            Debug.LogWarning("BackPos1 not available for fade animation!");
        }

        if (backPos2 != null && backPos2.GetComponent<SpriteRenderer>() != null)
        {
            backPos2.GetComponent<SpriteRenderer>().DOFade(1f, 2f);
        }
        else
        {
            Debug.LogWarning("BackPos2 not available for fade animation!");
        }
    }

    private void Awake()
    {
        Instance = this;
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        // Find BackPos objects for the initial level
        backPos1 = GameObject.FindGameObjectWithTag("BackPos1");
        backPos2 = GameObject.FindGameObjectWithTag("BackPos2");

        // Set initial BackPos states
        if (backPos1 != null && backPos1.GetComponent<SpriteRenderer>() != null)
        {
            backPos1.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        }
        else
        {
            Debug.LogWarning("BackPos1 not found or missing SpriteRenderer!");
        }

        if (backPos2 != null && backPos2.GetComponent<SpriteRenderer>() != null)
        {
            backPos2.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        }
        else
        {
            Debug.LogWarning("BackPos2 not found or missing SpriteRenderer!");
        }

        halfBorder.SetActive(true);
    }

    public void Start()
    {
        if (currentLevel == 0)
        {
            GameObject.FindAnyObjectByType<CutSceneManager>().StartCutScene();
        }
        // Don't spawn collectibles immediately - let LevelAddition handle the initial level setup
        // SpawnCollectibles(); // This will be called after the initial level drawing is complete
    }

    public void SpawnIn()
    {
        SpawnCollectibles();
    }

    #region Collectibles
    public bool AllCollectiblesCollected()
    {
        return activeCollectibles.Count == 0;
    }

    public void CollectItem(Collectible collected)
    {
        activeCollectibles.Remove(collected.gameObject);
        Destroy(collected.gameObject);
    }


    private void SpawnCollectibles()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("Collectible"))
        {
            Destroy(obj);
        }

        activeCollectibles.Clear();

        int spawned = 0;
        int attempts = 0;
        int totalToSpawn = 3 + currentLevel;

        while (spawned < totalToSpawn && attempts < maxAttempts)
        {
            attempts++;
            float x = Random.Range(topLeftSpawnArea.position.x, bottomRightSpawnArea.position.x);
            float y = Random.Range(bottomRightSpawnArea.position.y, topLeftSpawnArea.position.y);
            Vector3 spawnPos = new(x, y, -44.3f);
            bool blocked = Physics2D.OverlapCircle((Vector2)spawnPos, 0.3f, spawnBlockingLayers);
            if (!blocked)
            {
                GameObject newItem = Instantiate(collectiblePrefab, spawnPos, Quaternion.identity);
                newItem.tag = "Collectible";
                activeCollectibles.Add(newItem);
                spawned++;
            }
        }

        if (spawned < totalToSpawn)
        {
            Debug.LogWarning($"Only spawned {spawned} of {totalToSpawn} collectibles. Adjust area or LayerMask.");
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

        // Reset level state for new level
        FirstHalfDone = false;
        halfBorder.SetActive(true);

        GameObject player = GameObject.FindAnyObjectByType<PlainController>().gameObject;
        player.GetComponent<PlainController>().isinanim = false;
        player.GetComponent<Rigidbody2D>().gravityScale = player.GetComponent<PlainController>().gravity;

        // Call LevelAddition.NextLevel which will handle the drawing animation
        LevelAddition.Instance.NextLevel(currentLevel);

        // Spawn collectibles after level drawing is complete
        StartCoroutine(WaitForLevelDrawingAndSpawnCollectibles());

        // Find new BackPos objects for the new level
        backPos1 = GameObject.FindGameObjectWithTag("BackPos1");
        backPos2 = GameObject.FindGameObjectWithTag("BackPos2");

        // Set initial BackPos states
        if (backPos1 != null && backPos1.GetComponent<SpriteRenderer>() != null)
        {
            backPos1.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        }
        if (backPos2 != null && backPos2.GetComponent<SpriteRenderer>() != null)
        {
            backPos2.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        }

        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();
    }

    private IEnumerator WaitForLevelDrawingAndSpawnCollectibles()
    {
        // Wait for the level drawing to complete
        yield return new WaitUntil(() => LevelAddition.Instance != null && !LevelAddition.Instance.IsDrawingLevel);

        // Small delay to ensure everything is properly set up
        yield return new WaitForSeconds(0.1f);

        // Now spawn collectibles
        SpawnCollectibles();
    }

}
