using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToMenu : MonoBehaviour
{
    void Start()
    {
        Invoke("LoadMenuScene", 10f);
    }

    void LoadMenuScene()
    {
        Destroy(GameObject.FindAnyObjectByType<dontdestroy>().gameObject);
        SceneManager.LoadScene("MenuScene");
    }
}
