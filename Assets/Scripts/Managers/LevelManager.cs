using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public Transform startPoint;
    public GameObject collectiblePrefab;
    public int currentLevel = 0;
    private List<GameObject> activeCollectibles = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
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

        for (int i = 0; i < 3 + currentLevel; i++)
        {
            Vector2 pos = new Vector2(Random.Range(-6f, 6f), Random.Range(-3f, 3f));
            GameObject newItem = Instantiate(collectiblePrefab, pos, Quaternion.identity);
            newItem.tag = "Collectible";
            activeCollectibles.Add(newItem);
        }
    }
    #endregion
    public void NextLevel()
    {
        currentLevel++;
        SpawnCollectibles();
        LevelAddition.Instance.NextLevel(currentLevel);
    }

}
