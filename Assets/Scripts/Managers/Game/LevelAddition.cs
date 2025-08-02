using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LevelAddition : MonoBehaviour
{
    public static LevelAddition Instance;

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

        // Wait a short moment for everything to initialize
        yield return new WaitForSeconds(0.1f);

        // Set up the first level
        NextLevel(LevelManager.Instance.currentLevel);

        // Wait for drawing to complete, then tell LevelManager to spawn cows
        yield return new WaitUntil(() => !isDrawingLevel);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SpawnIn();
        }
    }

    public void NextLevel(int level)
    {
        Debug.Log($"Switching to level {level}");

        // First, deactivate all levels
        for (int i = 0; i < levelObjects.Count; i++)
        {
            levelObjects[i].SetActive(false);
        }

        // Check if level exists
        if (level >= 0 && level < levelObjects.Count)
        {
            // Activate the new level
            levelObjects[level].SetActive(true);

            // Start the drawing process for this level
            StartCoroutine(DrawLevelObstacles(levelObjects[level]));
        }
        else
        {
            Debug.LogWarning($"Level {level} does not exist!");
        }
    }

    private IEnumerator DrawLevelObstacles(GameObject levelObject)
    {
        isDrawingLevel = true;

        // Pause the game systems
        PauseGameSystems(true);

        // Find all obstacles in the level with "Obstacle" tag
        List<GameObject> obstacles = new List<GameObject>();
        Transform[] allTransforms = levelObject.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allTransforms)
        {
            if (t.gameObject.CompareTag("Obstacle"))
            {
                // Initially hide the obstacle
                t.gameObject.SetActive(false);
                obstacles.Add(t.gameObject);
            }
        }

        Debug.Log($"Found {obstacles.Count} obstacles to draw in level");

        // Wait a moment before starting to draw
        yield return new WaitForSeconds(0.5f);

        // Draw all obstacles if hand drawing is available
        if (HandDrawing.Instance != null && obstacles.Count > 0)
        {
            bool drawingComplete = false;

            // Use the hand drawing system to draw all obstacles
            HandDrawing.Instance.DrawMultipleObjects(obstacles.ToArray(), () =>
            {
                drawingComplete = true;
            });

            // Wait for drawing to complete
            yield return new WaitUntil(() => drawingComplete);
        }
        else
        {
            // Fallback: just activate all obstacles if hand drawing is not available
            foreach (GameObject obstacle in obstacles)
            {
                obstacle.SetActive(true);
            }

            if (HandDrawing.Instance == null)
            {
                Debug.LogWarning("HandDrawing.Instance not found! Obstacles will appear without drawing animation.");
            }
        }

        // Small delay before resuming gameplay
        yield return new WaitForSeconds(0.3f);

        // Resume the game systems
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
                PlainController.Instance.GetComponent<Rigidbody2D>().gravityScale = 0f;
            }
            else
            {
                // Restore gravity
                PlainController.Instance.GetComponent<Rigidbody2D>().gravityScale = PlainController.Instance.gravity;
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
    public bool IsDrawingLevel => isDrawingLevel;
}
