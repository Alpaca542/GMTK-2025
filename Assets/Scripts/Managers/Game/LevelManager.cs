using UnityEngine;
using System.Collections.Generic;

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
    [SerializeField] LevelSwitchAnimation levelSwitchAnimation;

    private void Awake()
    {
        Instance = this;
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 0);
    }

    public void Start()
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
        levelSwitchAnimation.AnimateLevelSwitch();
        Invoke(nameof(SwitchFinal), 3f);
    }
    private void SwitchFinal()
    {
        currentLevel++;
        LevelAddition.Instance.NextLevel(currentLevel);
        SpawnCollectibles();
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();
    }

}
