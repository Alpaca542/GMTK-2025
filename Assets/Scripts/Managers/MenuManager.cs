using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private string loseSceneName = "LoseScene";

    private AnimatedTransition transition;

    void Start()
    {
        transition = FindAnyObjectByType<AnimatedTransition>();
        if (!transition)
        {
            Debug.LogError("AnimatedTransition not found in the scene.");
        }
        transition.StartTransitionEnding();
    }

    public void TransitionToGame()
    {
        Debug.Log("Game Started");
        StartCoroutine(TransitionToScene(gameSceneName));
    }

    public void TransitionToLose()
    {
        Debug.Log("Transitioning to Lose Scene");
        StartCoroutine(TransitionToScene(loseSceneName));
    }

    public void TransitionToWin()
    {
        Debug.Log("Transitioning to Win Scene");
        StartCoroutine(TransitionToScene(winSceneName));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        if (transition)
        {
            transition.StartTransitionBeginning();
            yield return new WaitForSeconds(transition.transitionDuration);
        }

        SceneManager.LoadScene(sceneName);
    }
}
