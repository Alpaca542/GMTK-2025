using EZCameraShake;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private string loseSceneName = "LoseScene";
    public CameraShaker Sheker;

    public AnimatedTransition transition;

    void Start()
    {
        transition = AnimatedTransition.instance;
        if (!transition)
        {
            Debug.LogError("AnimatedTransition not found in the scene.");
        }
    }

    public void Play()
    {
        Debug.Log("Game Started");
        TransitionToScene(gameSceneName);
    }

    public void Exit()
    {
        transition.Exit();
    }

    public void FreakOut()
    {
        Sheker.StartShake(1, 10, 2f);
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
