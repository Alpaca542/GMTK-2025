using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

        // Wait a short moment for everything to initialize
        yield return new WaitForSeconds(0.1f);

        // For the first level, we want a smooth introduction
        // Set up the first level without the full transition animation
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
            // Fallback: just tell LevelManager to spawn cows
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SpawnIn();
            }
        }
    }

    private IEnumerator DrawFirstLevelObstacles(GameObject levelObject)
    {
        isDrawingLevel = true;

        // Brief pause for first level setup
        yield return new WaitForSeconds(0.5f);

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
            // Fallback: activate all obstacles
            foreach (GameObject obstacle in obstacles)
            {
                obstacle.SetActive(true);
            }

            if (HandDrawing.Instance == null)
            {
                Debug.LogWarning("HandDrawing.Instance not found! Obstacles will appear without drawing animation.");
            }

            yield return new WaitForSeconds(1f);
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

        // Brief pause before resuming
        yield return new WaitForSeconds(0.3f);

        // Resume game systems
        PauseGameSystems(false);
        isDrawingLevel = false;

        // Now tell LevelManager to spawn cows
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SpawnIn();
        }
    }

    public void NextLevel(int level)
    {
        Debug.Log($"Switching to level {level}");

        // First, deactivate all levels
        if (FirstLevel)
        {
            foreach (GameObject obj in levelObjects)
            {
                obj.SetActive(false);
            }
        }
        else
        {
            foreach (FadeOut fd in FindObjectsByType<FadeOut>(FindObjectsSortMode.None))
            {
                fd.FadeMeOut();
            }
        }
        FirstLevel = false;
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

        // Pause the game systems first
        PauseGameSystems(true);

        // Store original camera values
        Camera mainCamera = Camera.main;
        Vector3 originalCameraPosition = mainCamera.transform.position;
        float originalCameraSize = mainCamera.orthographicSize;

        // Get camera components and disable them properly
        PlayerFollow playerFollow = mainCamera.GetComponent<PlayerFollow>();
        CameraZoom cameraZoom = mainCamera.GetComponent<CameraZoom>();

        // Set transition mode on camera components
        if (playerFollow != null)
        {
            playerFollow.SetTransitionMode(true);
            playerFollow.enabled = false;
        }
        if (cameraZoom != null)
        {
            cameraZoom.SetTransitionMode(true);
            cameraZoom.enabled = false;
        }

        // Kill any existing camera animations to prevent conflicts
        mainCamera.DOKill();

        Debug.Log("Starting level transition - Camera zoom out phase");

        // Phase 1: Camera zoom out to overview
        bool cameraZoomComplete = false;

        // Move camera to zoom out position and adjust FOV with smooth easing
        if (LevelManager.Instance.zoomedOutPosition != null)
        {
            mainCamera.transform.DOMove(LevelManager.Instance.zoomedOutPosition.position, 2.0f)
                .SetEase(Ease.OutCubic);
        }

        mainCamera.DOOrthoSize(LevelManager.Instance.zoomedOutFOV, 2.0f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => cameraZoomComplete = true);

        // Wait for camera animation to complete
        yield return new WaitUntil(() => cameraZoomComplete);
        yield return new WaitForSeconds(0.8f); // Longer pause for player to see overview

        Debug.Log("Camera zoom out complete - Starting level fade out");

        // Phase 2: Fade out current level (only if not first level)
        if (!FirstLevel)
        {
            // Count how many objects will fade out
            FadeOut[] fadeOuts = FindObjectsByType<FadeOut>(FindObjectsSortMode.None);

            if (fadeOuts.Length > 0)
            {
                Debug.Log($"Starting fade out for {fadeOuts.Length} objects");

                bool fadeOutComplete = false;

                // Create callback for fade out completion
                System.Action onFadeOutComplete = null;
                onFadeOutComplete = () =>
                {
                    fadeOutComplete = true;
                    FadeOut.OnFadeOutComplete -= onFadeOutComplete;
                };
                FadeOut.OnFadeOutComplete += onFadeOutComplete;

                // Reset counter and trigger all fade outs
                FadeOut.ResetFadeOutCounter();
                foreach (FadeOut fd in fadeOuts)
                {
                    fd.FadeMeOut();
                }

                // Wait for fade outs to complete
                yield return new WaitUntil(() => fadeOutComplete);
                yield return new WaitForSeconds(0.2f); // Small buffer time
            }

            Debug.Log("Level fade out complete");
        }

        // Phase 3: Prepare new level obstacles
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

        // Phase 4: Hand drawing animation for new obstacles
        if (HandDrawing.Instance != null && obstacles.Count > 0)
        {
            bool drawingComplete = false;

            Debug.Log("Starting hand drawing animation");

            // Use the hand drawing system to draw all obstacles
            HandDrawing.Instance.DrawMultipleObjects(obstacles.ToArray(), () =>
            {
                drawingComplete = true;
                Debug.Log("Hand drawing animation complete");
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

            // Wait a bit to simulate drawing time if no hand drawing system
            yield return new WaitForSeconds(2f);
        }

        Debug.Log("Obstacles drawn - Starting camera zoom in");
        Debug.Log($"Camera original position: {originalCameraPosition}, original size: {originalCameraSize}");
        Debug.Log($"Camera current position: {mainCamera.transform.position}, current size: {mainCamera.orthographicSize}");

        // Phase 5: Camera zoom back in to gameplay view
        bool cameraZoomInComplete = false;

        // Kill any remaining camera animations before starting zoom in
        mainCamera.DOKill();

        // Return camera to original position and size with smooth easing
        var positionTween = mainCamera.transform.DOMove(originalCameraPosition, 2.0f)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() => Debug.Log("Camera position animation completed"));

        var sizeTween = mainCamera.DOOrthoSize(originalCameraSize, 2.0f)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() =>
            {
                cameraZoomInComplete = true;
                Debug.Log($"Camera zoom in animation completed - Final size: {mainCamera.orthographicSize}");
            });

        // Wait for camera to return
        yield return new WaitUntil(() => cameraZoomInComplete);

        Debug.Log("Camera zoom in complete - Re-enabling camera systems");

        // Clear transition mode and re-enable camera systems
        if (playerFollow != null)
        {
            playerFollow.SetTransitionMode(false);
            playerFollow.enabled = true;
        }
        if (cameraZoom != null)
        {
            cameraZoom.SetTransitionMode(false);
            cameraZoom.enabled = true;
            // Force the camera zoom to start its normal behavior after transition
            cameraZoom.ForceStartZoom();
        }

        // Double-check that camera is at the correct size (safety mechanism)
        if (Mathf.Abs(mainCamera.orthographicSize - originalCameraSize) > 0.1f)
        {
            Debug.LogWarning($"Camera size mismatch detected! Expected: {originalCameraSize}, Actual: {mainCamera.orthographicSize}. Correcting...");
            mainCamera.orthographicSize = originalCameraSize;
        }

        // Slightly longer delay before resuming gameplay for polish
        yield return new WaitForSeconds(0.5f);

        Debug.Log("Level transition complete - Resuming gameplay");

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
