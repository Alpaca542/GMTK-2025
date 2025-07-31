using UnityEngine;

public class LevelAddition : MonoBehaviour
{
    public static LevelAddition Instance;
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
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i == level)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
