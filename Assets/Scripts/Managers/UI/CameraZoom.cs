using UnityEngine;
using DG.Tweening; // Make sure you have DOTween imported

public class CameraZoom : MonoBehaviour
{
    public float zoomedInSize = 43f;
    public float zoomedOutSize = 88f;
    public float zoomSpeed = 1.5f; // Made faster for better responsiveness

    private bool isLevelTransitioning = false;
    private bool hasStartedInitialZoom = false;

    void Start()
    {
        Camera.main.orthographicSize = zoomedOutSize;
        // Start zoom immediately instead of waiting
        StartZoom();
    }

    void StartZoom()
    {
        // Check if we're in a level transition - if so, don't interfere
        if (isLevelTransitioning || (LevelAddition.Instance != null && LevelAddition.Instance.IsDrawingLevel))
        {
            // Don't reschedule - let the transition system handle it
            return;
        }

        hasStartedInitialZoom = true;
        // Tween the orthographic size using DOTween with better easing
        Camera.main.DOFieldOfView(zoomedInSize, zoomSpeed)
            .SetEase(Ease.OutQuart);
    }

    // Method to be called by LevelAddition to prevent interference
    public void SetTransitionMode(bool transitioning)
    {
        isLevelTransitioning = transitioning;

        if (transitioning)
        {
            Camera.main.DOKill();
        }
        else
        {
            ForceStartZoom();
        }
    }

    // Public method to manually start zoom (for use after transitions)
    public void ForceStartZoom()
    {
        if (!isLevelTransitioning)
        {
            Camera.main.DOFieldOfView(zoomedInSize, zoomSpeed)
                .SetEase(Ease.OutQuart);
        }
    }
}
