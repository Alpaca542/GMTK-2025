using UnityEngine;
using DG.Tweening; // Make sure you have DOTween imported

public class CameraZoom : MonoBehaviour
{
    public float zoomedInSize = 43f;
    public float zoomedOutSize = 88f;
    public float zoomSpeed = 2f;

    void Start()
    {
        Camera.main.orthographicSize = zoomedOutSize;
        Invoke(nameof(StartZoom), 1f);
    }

    void StartZoom()
    {
        // Tween the orthographic size using DOTween
        Camera.main.DOFieldOfView(zoomedInSize, zoomSpeed);
    }
}
