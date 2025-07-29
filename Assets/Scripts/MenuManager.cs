using UnityEngine.SceneManagement;
using UnityEngine;
public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private string loseSceneName = "LoseScene";

    private AnimatedTransition transition;
    void Start()
    {
        transition = GameObject.FindAnyObjectByType<AnimatedTransition>();
        if (transition == null)
        {
            Debug.LogError("AnimatedTransition not found in the scene.");
        }
    }
    public void TransitionToGame()
    {
        Debug.Log("Game Started");
    }
}
