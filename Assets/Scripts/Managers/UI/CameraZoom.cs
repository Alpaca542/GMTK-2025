using UnityEngine;
using DG.Tweening; // Make sure you have DOTween imported

public class CameraZoom : MonoBehaviour
{
    public float zoomedInSize = 43f;
    public float zoomedOutSize = 88f;
    public float zoomSpeed = 2f;

    private bool isLevelTransitioning = false;

    void Start()
    {
        Camera.main.orthographicSize = zoomedOutSize;
        Invoke(nameof(StartZoom), 1f);
    }

    void StartZoom()
    {
        // Check if we're in a level transition - if so, don't interfere
        if (LevelAddition.Instance != null && LevelAddition.Instance.IsDrawingLevel)
        {
            // Reschedule for later
            Invoke(nameof(StartZoom), 0.5f);
            return;
        }

        // Tween the orthographic size using DOTween (corrected from DOFieldOfView)
        Camera.main.DOOrthoSize(zoomedInSize, zoomSpeed);
    }

    // Method to be called by LevelAddition to prevent interference
    public void SetTransitionMode(bool transitioning)
    {
        isLevelTransitioning = transitioning;

        if (transitioning)
        {
            // Cancel any ongoing zoom animations when transitioning starts
            Camera.main.DOKill();
        }
    }

    // Public method to manually start zoom (for use after transitions)
    public void ForceStartZoom()
    {
        if (!isLevelTransitioning)
        {
            Camera.main.DOOrthoSize(zoomedInSize, zoomSpeed);
        }
    }
}
