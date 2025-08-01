using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScript : MonoBehaviour
{
    public float introLongLength;
    public float introStandardLength;
    public string loadAfterIntro;
    public GameObject deleteIfStandard;
    public string keyName = "watchedIntro";
    void Start()
    {
        string watched = PlayerPrefs.GetString(keyName, "");
        if(watched == "yes")
        {
            Invoke(nameof(LoadScene), introStandardLength);
            deleteIfStandard.SetActive(false);
        }
        else
        {
            Invoke(nameof(LoadScene), introLongLength);
            PlayerPrefs.SetString(keyName, "yes");
        }
    }
    public void LoadScene()
    {
        SceneManager.LoadSceneAsync(loadAfterIntro);
    }
}
