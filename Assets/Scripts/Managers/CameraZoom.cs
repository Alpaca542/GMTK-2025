using UnityEngine;

public class CameraZoom : MonoBehaviour {
    public float zoomedInSize = 5f;
    public float zoomedOutSize = 10f;
    public float zoomSpeed = 2f;
    private bool zoomingIn = false;

    void Start() {
        Camera.main.orthographicSize = zoomedOutSize;
        Invoke(nameof(StartZoom), 1f);
    }

    void StartZoom() {
        zoomingIn = true;
    }

    void Update() {
        if (zoomingIn) {
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, zoomedInSize, Time.deltaTime * zoomSpeed);
        }
    }
}
