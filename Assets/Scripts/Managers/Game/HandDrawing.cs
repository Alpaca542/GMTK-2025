using System.Collections;
using UnityEngine;
using DG.Tweening;

public class HandDrawing : MonoBehaviour
{
    [Header("Hand Animation Settings")]
    [SerializeField] private Transform handTransform;
    [SerializeField] private float drawingSpeed = 2.0f;
    [SerializeField] private float shakingIntensity = 0.1f;
    [SerializeField] private float shakingFrequency = 15f;
    [SerializeField] private float minShakeTime = 0.5f;
    [SerializeField] private float maxShakeTime = 2.0f;

    [Header("Movement Settings")]
    [SerializeField] private float approachDistance = 1.5f;
    [SerializeField] private float drawingHeight = 0.5f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio/Visual Effects")]
    [SerializeField] private ParticleSystem drawingParticles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip drawSound;
    [SerializeField] private AudioClip eraseSound;

    private Vector3 originalHandPosition;
    private Quaternion originalHandRotation;
    private Coroutine currentAnimationCoroutine;
    private bool isAnimating = false;
    private Sequence shakeSequence;

    void Start()
    {
        if (handTransform == null)
            handTransform = transform;

        originalHandPosition = handTransform.position;
        originalHandRotation = handTransform.rotation;
    }

    /// <summary>
    /// Animates the hand to draw or erase a 2D GameObject
    /// </summary>
    /// <param name="targetObject">The 2D GameObject to draw/erase</param>
    /// <param name="isDraw">True for drawing, false for erasing</param>
    public void AnimateDrawing(GameObject targetObject, bool isDraw)
    {
        if (isAnimating)
        {
            StopCurrentAnimation();
        }

        if (targetObject == null)
        {
            Debug.LogWarning("Target object is null!");
            return;
        }

        currentAnimationCoroutine = StartCoroutine(DrawingAnimationCoroutine(targetObject, isDraw));
    }

    /// <summary>
    /// Stops the current drawing animation and returns hand to original position
    /// </summary>
    public void StopDrawing()
    {
        StopCurrentAnimation();
        ReturnToOriginalPosition();
    }

    private IEnumerator DrawingAnimationCoroutine(GameObject targetObject, bool isDraw)
    {
        isAnimating = true;

        // Get object bounds for movement planning
        Bounds objectBounds = GetObjectBounds(targetObject);
        Vector3 objectCenter = objectBounds.center;
        Vector3 objectSize = objectBounds.size;

        // Calculate drawing path based on object size
        Vector3[] drawingPath = CalculateDrawingPath(objectBounds);

        // Play appropriate sound
        PlayDrawingSound(isDraw);

        // Start particle effects if available
        if (drawingParticles != null)
        {
            var main = drawingParticles.main;
            main.startColor = isDraw ? Color.black : Color.white;
            drawingParticles.Play();
        }

        // Phase 1: Move to starting position
        yield return StartCoroutine(MoveToPosition(drawingPath[0], 0.8f));

        // Phase 2: Drawing/Erasing animation
        yield return StartCoroutine(ExecuteDrawingPath(drawingPath, isDraw));

        ApplyDrawingEffect(targetObject, isDraw);

        // Phase 3: Return to original position
        yield return StartCoroutine(MoveToPosition(originalHandPosition, 0.6f));

        // Stop effects
        if (drawingParticles != null)
            drawingParticles.Stop();

        isAnimating = false;
    }

    private Vector3[] CalculateDrawingPath(Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;

        // For small objects, just hover over the center
        if (size.magnitude < 2.0f)
        {
            Vector3 hoverPosition = new Vector3(center.x, center.y, center.z - drawingHeight);
            return new Vector3[] { hoverPosition };
        }

        // For larger objects, create a path from one end to another
        Vector3 startPoint, endPoint;

        // Determine the best drawing direction based on object shape
        if (size.x > size.y)
        {
            // Draw horizontally for wide objects
            startPoint = new Vector3(center.x - size.x * 0.4f, center.y, center.z - drawingHeight);
            endPoint = new Vector3(center.x + size.x * 0.4f, center.y, center.z - drawingHeight);
        }
        else
        {
            // Draw vertically for tall objects
            startPoint = new Vector3(center.x, center.y + size.y * 0.4f, center.z - drawingHeight);
            endPoint = new Vector3(center.x, center.y - size.y * 0.4f, center.z - drawingHeight);
        }

        // Add some intermediate points for more complex paths
        Vector3 midPoint = Vector3.Lerp(startPoint, endPoint, 0.5f);
        return new Vector3[] { startPoint, midPoint, endPoint };
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = handTransform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float curveValue = movementCurve.Evaluate(progress);

            handTransform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
            yield return null;
        }

        handTransform.position = targetPosition;
    }

    private IEnumerator ExecuteDrawingPath(Vector3[] path, bool isDraw)
    {
        // Start shaking animation
        StartRealisticShaking(isDraw);

        // Move through the drawing path
        for (int i = 0; i < path.Length; i++)
        {
            if (i > 0)
            {
                // Calculate time based on distance and drawing speed
                float distance = Vector3.Distance(path[i - 1], path[i]);
                float moveTime = distance / drawingSpeed;

                yield return StartCoroutine(MoveToPosition(path[i], moveTime));
            }

            // Add some variation in timing for realism
            float pauseTime = Random.Range(0.1f, 0.3f);
            yield return new WaitForSeconds(pauseTime);
        }

        // Stop shaking
        StopShaking();
    }

    private void StartRealisticShaking(bool isDraw)
    {
        StopShaking();

        // Different shaking patterns for drawing vs erasing
        float intensity = isDraw ? shakingIntensity : shakingIntensity * 1.5f;
        float frequency = isDraw ? shakingFrequency : shakingFrequency * 0.7f;

        shakeSequence = DOTween.Sequence();

        // Create realistic hand tremor
        for (int i = 0; i < 50; i++) // 50 shake iterations
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity),
                Random.Range(-intensity * 0.3f, intensity * 0.3f)
            );

            shakeSequence.Append(handTransform.DOLocalMove(randomOffset, 1f / frequency)
                .SetRelative(true)
                .SetEase(Ease.InOutSine));
        }

        shakeSequence.SetLoops(-1, LoopType.Restart);
    }

    private void StopShaking()
    {
        if (shakeSequence != null && shakeSequence.IsActive())
        {
            shakeSequence.Kill();
            shakeSequence = null;
        }
    }

    private Bounds GetObjectBounds(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }

        // Fallback: use collider bounds
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds;
        }

        // Fallback: use transform position with default size
        return new Bounds(obj.transform.position, Vector3.one);
    }

    private void PlayDrawingSound(bool isDraw)
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = isDraw ? drawSound : eraseSound;
        if (clipToPlay != null)
        {
            audioSource.clip = clipToPlay;
            audioSource.Play();
        }
    }

    private void StopCurrentAnimation()
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }

        StopShaking();
        isAnimating = false;
    }

    private void ReturnToOriginalPosition()
    {
        handTransform.DOMove(originalHandPosition, 0.8f)
            .SetEase(Ease.OutQuart);
        handTransform.DORotateQuaternion(originalHandRotation, 0.8f)
            .SetEase(Ease.OutQuart);
    }
    private void ApplyDrawingEffect(GameObject targetObject, bool isDraw)
    {
        if (targetObject == null) return;

        if (isDraw)
        {
            // Drawing: Turn the object on
            targetObject.SetActive(true);
            Debug.Log($"Drew object: {targetObject.name} - Object is now visible");
        }
        else
        {
            // Erasing: Turn the object off
            targetObject.SetActive(false);
            Debug.Log($"Erased object: {targetObject.name} - Object is now hidden");
        }
    }
    void OnDestroy()
    {
        StopCurrentAnimation();
    }

    // Public properties for runtime adjustments
    public bool IsAnimating => isAnimating;
    public float DrawingSpeed
    {
        get => drawingSpeed;
        set => drawingSpeed = Mathf.Max(0.1f, value);
    }
    public float ShakingIntensity
    {
        get => shakingIntensity;
        set => shakingIntensity = Mathf.Max(0f, value);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Visualize the drawing area in the editor
        if (handTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(handTransform.position, approachDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(originalHandPosition, 0.1f);
        }
    }
#endif
}
