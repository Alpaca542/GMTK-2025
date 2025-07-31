using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private string loseSceneName = "LoseScene";

    public AnimatedTransition transition;

    void Start()
    {
        transition = AnimatedTransition.instance;
        if (!transition)
        {
            Debug.LogError("AnimatedTransition not found in the scene.");
        }
    }

    public void TransitionToGame()
    {
        Debug.Log("Game Started");
        TransitionToScene(gameSceneName);
    }

    public void TransitionToLose()
    {
        Debug.Log("Transitioning to Lose Scene");
        TransitionToScene(loseSceneName);
    }

    public void TransitionToWin()
    {
        Debug.Log("Transitioning to Win Scene");
        TransitionToScene(winSceneName);
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
}
