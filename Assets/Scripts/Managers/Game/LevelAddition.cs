using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEditor.SearchService;

public class LevelAddition : MonoBehaviour
{
    public static LevelAddition Instance;
    public bool FirstLevel = true;
    [SerializeField] private List<GameObject> levelObjects = new List<GameObject>();
    private bool isDrawingLevel = false;
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
        // Set up the initial level with drawing animation
        StartCoroutine(InitializeFirstLevel());
    }

    private IEnumerator InitializeFirstLevel()
    {
        // Wait for LevelManager to be ready
        yield return new WaitUntil(() => LevelManager.Instance != null);

        // Immediate initialization - no unnecessary waiting
        Debug.Log($"Initializing first level {LevelManager.Instance.currentLevel}");

        // First, deactivate all levels
        foreach (GameObject obj in levelObjects)
        {
            obj.SetActive(false);
        }

        // Check if level exists and activate it
        int level = LevelManager.Instance.currentLevel;
        if (level >= 0 && level < levelObjects.Count)
        {
            levelObjects[level].SetActive(true);

            // For first level, we'll do a simpler animation sequence
            StartCoroutine(DrawFirstLevelObstacles(levelObjects[level]));
        }
        else
        {
            Debug.LogWarning($"Level {level} does not exist!");
            SceneManager.LoadScene("Ending");
        }
    }

    private IEnumerator DrawFirstLevelObstacles(GameObject levelObject)
    {
        isDrawingLevel = true;

        // Notify camera components we're in transition mode (but don't disable them for first level)
        Camera mainCamera = Camera.main;
        PlayerFollow playerFollow = mainCamera.GetComponent<PlayerFollow>();
        CameraZoom cameraZoom = mainCamera.GetComponent<CameraZoom>();

        if (playerFollow != null)
        {
            playerFollow.SetTransitionMode(true);
        }
        if (cameraZoom != null)
        {
            cameraZoom.SetTransitionMode(true);
        }

        // Pause game systems but don't do camera animations for first level
        PauseGameSystems(true);

        // Prepare obstacles
        List<GameObject> obstacles = new List<GameObject>();
        Transform[] allTransforms = levelObject.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allTransforms)
        {
            if (t.gameObject.CompareTag("Obstacle"))
            {
                t.gameObject.SetActive(false);
                obstacles.Add(t.gameObject);
            }
        }

        Debug.Log($"Found {obstacles.Count} obstacles to draw in first level");

        // Hand drawing animation (without camera movement for first level)
        if (HandDrawing.Instance != null && obstacles.Count > 0)
        {
            bool drawingComplete = false;

            Debug.Log("Starting hand drawing animation for first level");

            HandDrawing.Instance.DrawMultipleObjects(obstacles.ToArray(), () =>
            {
                drawingComplete = true;
                Debug.Log("First level hand drawing animation complete");
            });

            yield return new WaitUntil(() => drawingComplete);
        }
        else
        {
            // Fallback: activate all obstacles immediately - no waiting
            foreach (GameObject obstacle in obstacles)
            {
                obstacle.SetActive(true);
            }

            if (HandDrawing.Instance == null)
            {
                Debug.LogWarning("HandDrawing.Instance not found! Obstacles will appear without drawing animation.");
            }
        }

        // Clear transition mode for camera components
        if (playerFollow != null)
        {
            playerFollow.SetTransitionMode(false);
        }
        if (cameraZoom != null)
        {
            cameraZoom.SetTransitionMode(false);
            // Let CameraZoom handle its own startup zoom
        }

        // Resume game systems immediately
        PauseGameSystems(false);
        isDrawingLevel = false;

        // Now tell LevelManager to spawn cows immediately
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SpawnIn();
        }
    }

    public void NextLevel(int level, GameObject player)
    {
        Debug.Log($"Switching to level {level}");

        // Check if level exists first
        if (level >= 0 && level < levelObjects.Count)
        {
            // Start the transition process - fade out first, then activate new level
            StartCoroutine(TransitionToLevel(level, player));
        }
        else
        {
            Debug.LogWarning($"Level {level} does not exist!");
        }
    }

    private IEnumerator TransitionToLevel(int level, GameObject player)
    {
        isDrawingLevel = true;

        // Pause the game systems first
        PauseGameSystems(true);

        // Get camera components and set transition mode
        Camera mainCamera = Camera.main;
        PlayerFollow playerFollow = mainCamera.GetComponent<PlayerFollow>();
        CameraZoom cameraZoom = mainCamera.GetComponent<CameraZoom>();

        // Set transition mode on camera components
        if (playerFollow != null)
        {
            playerFollow.SetTransitionMode(true);
        }
        if (cameraZoom != null)
        {
            cameraZoom.SetTransitionMode(true);
        }

        Debug.Log("Starting level transition - Fade out phase");
        Camera.main.DOFieldOfView(88f, 0.5f).SetEase(Ease.OutQuart).OnComplete(() =>
        {
            Debug.Log("Camera zoomed out for level transition");
        });
        // Phase 1: Fade out current level FIRST (only if not first level)
        if (!FirstLevel)
        {
            FadeOut[] fadeOuts = FindObjectsByType<FadeOut>(FindObjectsSortMode.None);

            if (fadeOuts.Length > 0)
            {
                Debug.Log($"Starting fade out for {fadeOuts.Length} objects");

                foreach (FadeOut fd in fadeOuts)
                {
                    fd.FadeMeOut();
                }

                // Wait for fade outs to complete before proceeding
                yield return new WaitForSeconds(1.2f); // Adjust this wait time as needed
                Debug.Log("Level fade out complete");
            }
        }
        else
        {
            // For first level, just deactivate all levels
            foreach (GameObject obj in levelObjects)
            {
                obj.SetActive(false);
            }
        }
        FirstLevel = false;

        Debug.Log("Fade out complete - Activating new level");

        // Phase 2: NOW activate the new level AFTER fade out is complete
        levelObjects[level].SetActive(true);

        // Phase 3: Prepare new level obstacles
        List<GameObject> obstacles = new List<GameObject>();
        Transform[] allTransforms = levelObjects[level].GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allTransforms)
        {
            if (t.gameObject.CompareTag("Obstacle"))
            {
                t.gameObject.SetActive(false);
                obstacles.Add(t.gameObject);
            }
        }

        Debug.Log($"Found {obstacles.Count} obstacles to draw in level");

        // Phase 4: Hand drawing animation
        if (HandDrawing.Instance != null && obstacles.Count > 0)
        {
            bool drawingComplete = false;

            Debug.Log("Starting hand drawing animation");
            obstacles.Add(player);
            HandDrawing.Instance.DrawMultipleObjects(obstacles.ToArray(), () =>
            {
                drawingComplete = true;
                Debug.Log("Hand drawing animation complete");
            });

            yield return new WaitUntil(() => drawingComplete);

        }
        else
        {
            // Fallback: activate all obstacles immediately
            foreach (GameObject obstacle in obstacles)
            {
                obstacle.SetActive(true);
            }

            if (HandDrawing.Instance == null)
            {
                Debug.LogWarning("HandDrawing.Instance not found! Obstacles will appear without drawing animation.");
            }
        }

        Debug.Log("Obstacles drawn - Clearing transition mode");

        // Clear transition mode for camera components
        if (playerFollow != null)
        {
            playerFollow.SetTransitionMode(false);
        }
        if (cameraZoom != null)
        {
            cameraZoom.SetTransitionMode(false);
        }

        Debug.Log("Level transition complete - Resuming gameplay");

        // Resume the game systems immediately
        PauseGameSystems(false);
        isDrawingLevel = false;
    }

    private void PauseGameSystems(bool pause)
    {
        // Pause/Resume PlainController
        if (PlainController.Instance != null)
        {
            PlainController.Instance.isinanim = pause;

            if (pause)
            {
                // Stop player movement and set gravity to 0 to freeze in place
                PlainController.Instance.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                PlainController.Instance.GetComponent<Rigidbody2D>().angularVelocity = 0f;
                PlainController.Instance.GetComponent<Rigidbody2D>().gravityScale = 0f;

                // Disable player input during drawing
                PlainController.Instance.enabled = false;
            }
            else
            {
                // Restore gravity and re-enable player
                PlainController.Instance.GetComponent<Rigidbody2D>().gravityScale = PlainController.Instance.gravity;
                PlainController.Instance.enabled = true;
                PlainController.Instance.isinanim = false; // Make sure animation flag is cleared
            }
        }

        // Pause/Resume animated obstacles
        PauseAnimatedObstacles(pause);

        Debug.Log($"Game systems {(pause ? "paused" : "resumed")} for level drawing");
    }

    private void PauseAnimatedObstacles(bool pause)
    {
        // Instead of pausing ALL DOTween animations (which would affect the hand drawing),
        // we'll specifically disable the components that create animations

        // Pause/Resume MovingObject and RotatingObstacle components
        // These will stop creating new DOTween animations when disabled
        MovingObject[] movingObjects = FindObjectsByType<MovingObject>(FindObjectsSortMode.None);
        foreach (MovingObject movingObj in movingObjects)
        {
            movingObj.enabled = !pause;
        }

        RotatingObstacle[] rotatingObstacles = FindObjectsByType<RotatingObstacle>(FindObjectsSortMode.None);
        foreach (RotatingObstacle rotatingObstacle in rotatingObstacles)
        {
            rotatingObstacle.enabled = !pause;
        }

        // Pause/Resume ConstantlyRotatingObject components
        ConstantlyRotatingObject[] rotatingObjects = FindObjectsByType<ConstantlyRotatingObject>(FindObjectsSortMode.None);
        foreach (ConstantlyRotatingObject rotatingObj in rotatingObjects)
        {
            rotatingObj.enabled = !pause;
        }

        // You can add more specific obstacle types here if needed
        // For example, if there are other Update-based animations
    }

    private Camera storedCamera;

    public bool IsDrawingLevel => isDrawingLevel;
}
