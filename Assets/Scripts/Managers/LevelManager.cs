using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour {
    public static LevelManager Instance;
    public Transform startPoint;
    public GameObject collectiblePrefab;
    public int currentLevel = 0;

    private void Awake() {
        Instance = this;
    }

    public void Start() {
        SpawnCollectibles();
    }

    public void CollectItem(Collectible collected) {
        
    }

    public void CompleteLoop() {
        currentLevel++;
        ResetPlane();
        SpawnCollectibles();
    }

    private void ResetPlane() {
        GameObject player = GameObject.FindWithTag("Player");
        player.transform.position = startPoint.position;
        player.transform.rotation = Quaternion.identity;
    }

    private void SpawnCollectibles() {
        for (int i = 0; i < 3 + currentLevel; i++) {
            Vector2 pos = new Vector2(Random.Range(-6f, 6f), Random.Range(-3f, 3f));
            Instantiate(collectiblePrefab, pos, Quaternion.identity);
        }
    }
}
