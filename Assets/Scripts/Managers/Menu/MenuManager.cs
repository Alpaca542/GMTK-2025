using EZCameraShake;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string menuSceneName = "MenuScene";
    public CameraShaker Sheker;
    public AudioSource freeeaaakkkk;
    private CameraShakeInstance shaker_I;
    public AudioSource src;

    public AnimatedTransition transition;

    void Start()
    {
        transition = AnimatedTransition.instance;
        if (!transition)
        {
            Debug.LogError("AnimatedTransition not found in the scene.");
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
            Screen.fullScreen = !Screen.fullScreen;
    }

    public void Play()
    {
        Debug.Log("Game Started");
        TransitionToScene(gameSceneName);
    }
    public void ExitMenu()
    {
        Debug.Log("gwahah");
        src.Stop();
        Invoke(nameof(ExitMenuInvoke), 1.5f);
    }
    void ExitMenuInvoke()
    {
        SceneManager.LoadSceneAsync(menuSceneName);
    }
    public void Exit()
    {
        transition.Exit();
    }

    public void FreakOut()
    {
        shaker_I = Sheker.StartShake(1, 10, 2f);
        Invoke(nameof(StopFreakOut), 16);
        freeeaaakkkk.Play();
    }

    public void StopFreakOut()
    {
        shaker_I.StartFadeOut(1f);
    }

    private void TransitionToScene(string sceneName)
    {
        if (transition)
        {
            transition.TransitionTo(sceneName);
        }
        else
        {
            Debug.LogError("Bruh");
        }
    }

    public void SetVolume(float volume)
    {
        Global.volume = volume;
        AudioListener.volume = volume;
    }
}
