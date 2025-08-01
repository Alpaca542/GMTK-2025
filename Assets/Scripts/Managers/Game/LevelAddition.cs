using System.Collections.Generic;
using UnityEngine;

public class LevelAddition : MonoBehaviour
{
    public static LevelAddition Instance;

    [SerializeField] private List<GameObject> levelObjects = new List<GameObject>();
    void Reset()
    {
        levelObjects.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            levelObjects.Add(transform.GetChild(i).gameObject);
        }
    }
    void Awake()
    {
        Instance = this;
        levelObjects.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            levelObjects.Add(transform.GetChild(i).gameObject);
        }
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
            levelObjects[i].SetActive(i == level);
        }
    }
}
