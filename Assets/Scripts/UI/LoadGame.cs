using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGame : MonoBehaviour
{
    public float AnimationDuration = 30f;
    public string SceneToLoad = "";
    void Start()
    {
        Invoke(nameof(Load), AnimationDuration);
    }
    void Load()
    {
        SceneManager.LoadSceneAsync(SceneToLoad);
    }
}
