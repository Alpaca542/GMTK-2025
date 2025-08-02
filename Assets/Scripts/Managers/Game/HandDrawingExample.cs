using UnityEngine;

/// <summary>
/// Example script demonstrating how to use the HandDrawing component
/// Attach this to any GameObject to test the hand drawing functionality
/// </summary>
public class HandDrawingExample : MonoBehaviour
{
    [Header("Example Setup")]
    [SerializeField] private HandDrawing handDrawing;
    [SerializeField] private GameObject[] targetObjects;
    [SerializeField] private KeyCode drawKey = KeyCode.D;
    [SerializeField] private KeyCode eraseKey = KeyCode.E;
    [SerializeField] private KeyCode stopKey = KeyCode.S;

    private int currentTargetIndex = 0;

    void Start()
    {
        // Auto-find HandDrawing component if not assigned
        if (handDrawing == null)
            handDrawing = FindAnyObjectByType<HandDrawing>();

        if (handDrawing == null)
        {
            Debug.LogError("HandDrawing component not found! Please assign it in the inspector or add it to a GameObject in the scene.");
            return;
        }

        // Auto-find target objects if not assigned
        if (targetObjects == null || targetObjects.Length == 0)
        {
            // Find all GameObjects with renderers (potential drawing targets)
            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            targetObjects = new GameObject[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                targetObjects[i] = renderers[i].gameObject;
            }
        }

        DisplayInstructions();
    }

    void Update()
    {
        if (targetObjects == null || targetObjects.Length == 0)
            return;

        // Handle input
        if (Input.GetKeyDown(drawKey))
        {
            DrawCurrentTarget();
        }
        else if (Input.GetKeyDown(eraseKey))
        {
            EraseCurrentTarget();
        }
        else if (Input.GetKeyDown(stopKey))
        {
            StopDrawing();
        }

        // Cycle through targets with number keys
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                SelectTarget(i - 1);
                break;
            }
        }

        // Cycle targets with arrow keys
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CycleTarget(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CycleTarget(-1);
        }
    }

    private void DrawCurrentTarget()
    {
        if (GetCurrentTarget() != null)
        {
            Debug.Log($"Drawing target: {GetCurrentTarget().name}");
            handDrawing.AnimateDrawing(GetCurrentTarget(), true);
        }
        else
        {
            Debug.LogWarning("No valid target selected!");
        }
    }

    private void EraseCurrentTarget()
    {
        if (GetCurrentTarget() != null)
        {
            Debug.Log($"Erasing target: {GetCurrentTarget().name}");
            handDrawing.AnimateDrawing(GetCurrentTarget(), false);
        }
        else
        {
            Debug.LogWarning("No valid target selected!");
        }
    }

    private void StopDrawing()
    {
        Debug.Log("Stopping hand drawing animation");
        handDrawing.StopDrawing();
    }

    private void SelectTarget(int index)
    {
        if (index >= 0 && index < targetObjects.Length)
        {
            currentTargetIndex = index;
            Debug.Log($"Selected target {currentTargetIndex + 1}: {GetCurrentTarget().name}");
        }
    }

    private void CycleTarget(int direction)
    {
        if (targetObjects.Length == 0) return;

        currentTargetIndex = (currentTargetIndex + direction) % targetObjects.Length;
        if (currentTargetIndex < 0)
            currentTargetIndex = targetObjects.Length - 1;

        Debug.Log($"Selected target {currentTargetIndex + 1}: {GetCurrentTarget().name}");
    }

    private GameObject GetCurrentTarget()
    {
        if (targetObjects == null || targetObjects.Length == 0 || currentTargetIndex >= targetObjects.Length)
            return null;

        return targetObjects[currentTargetIndex];
    }

    private void DisplayInstructions()
    {
        Debug.Log("=== Hand Drawing Example Controls ===");
        Debug.Log($"Press '{drawKey}' to draw the current target");
        Debug.Log($"Press '{eraseKey}' to erase the current target");
        Debug.Log($"Press '{stopKey}' to stop the current animation");
        Debug.Log("Press Left/Right arrows to cycle through targets");
        Debug.Log("Press number keys (1-9) to select specific targets");
        Debug.Log($"Current target count: {(targetObjects != null ? targetObjects.Length : 0)}");

        if (GetCurrentTarget() != null)
        {
            Debug.Log($"Current target: {GetCurrentTarget().name}");
        }
    }

    // Public methods for UI integration
    public void DrawTargetByIndex(int index)
    {
        SelectTarget(index);
        DrawCurrentTarget();
    }

    public void EraseTargetByIndex(int index)
    {
        SelectTarget(index);
        EraseCurrentTarget();
    }

    public void DrawSpecificTarget(GameObject target, bool isDraw)
    {
        if (target != null && handDrawing != null)
        {
            handDrawing.AnimateDrawing(target, isDraw);
        }
    }

    // Properties for runtime access
    public GameObject CurrentTarget => GetCurrentTarget();
    public int TargetCount => targetObjects != null ? targetObjects.Length : 0;
    public bool IsDrawing => handDrawing != null && handDrawing.IsAnimating;
}
