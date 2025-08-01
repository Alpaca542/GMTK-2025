using UnityEngine;
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
        Camera.main.transform.DOMove(new Vector3(0, 0, -10), 2f).SetEase(Ease.InOutSine);
        Camera.main.DOFieldOfView(88f, 2f).OnComplete(() =>
        {
            Camera.main.GetComponent<PlayerFollow>().enabled = true;
            Camera.main.GetComponent<CameraZoom>().enabled = true;
            Camera.main.DOFieldOfView(60f, 2f);
        });
        backPos1.GetComponent<SpriteRenderer>().DOFade(0f, 2f);
        backPos2.GetComponent<SpriteRenderer>().DOFade(1f, 2f);
    }

    private void Awake()
    {
        backPos1 = GameObject.FindGameObjectWithTag("BackPos1");
        backPos2 = GameObject.FindGameObjectWithTag("BackPos2");
        backPos1.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        backPos2.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        halfBorder.SetActive(true);
        Instance = this;
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
    }

    public void Start()
    {
        if (currentLevel == 0)
        {
            GameObject.FindAnyObjectByType<CutSceneManager>().StartCutScene();
        }
        SpawnCollectibles();
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
            Vector2 spawnPos = new(x, y);
            bool blocked = Physics2D.OverlapCircle(spawnPos, 0.3f, spawnBlockingLayers);
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlainController>().isinanim = false;
        player.GetComponent<Rigidbody2D>().gravityScale = player.GetComponent<PlainController>().gravity;
        LevelAddition.Instance.NextLevel(currentLevel);
        SpawnCollectibles();
        backPos1 = GameObject.FindGameObjectWithTag("BackPos1");
        backPos2 = GameObject.FindGameObjectWithTag("BackPos2");
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();
    }

}
