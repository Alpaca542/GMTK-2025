using System.Collections.Generic;
using UnityEngine;

public class LevelAddition : MonoBehaviour
{
    public static LevelAddition Instance;

    [SerializeField] private List<GameObject> levelObjects = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        NextLevel(LevelManager.Instance.currentLevel);
    }

    public void NextLevel(int level)
    {
        Debug.Log($"Switching to level {level}");
        for (int i = 0; i < levelObjects.Count; i++)
        {
            levelObjects[i].SetActive(i <= level);
        }
    }
}
