using System.Collections;
using UnityEngine;
using DG.Tweening;
using System;

public class HandDrawing : MonoBehaviour
{
    public static HandDrawing Instance;

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

    [SerializeField] private GameObject CurrentTrail;
    [SerializeField] private GameObject trailPrefab;

    void Start()
    {
        Instance = this;

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

        currentAnimationCoroutine = StartCoroutine(DrawingAnimationCoroutine(targetObject, isDraw, true));
    }

    /// <summary>
    /// Stops the current drawing animation and returns hand to original position
    /// </summary>
    public void StopDrawing()
    {
        StopCurrentAnimation();
        ReturnToOriginalPosition();
    }

    /// <summary>
    /// Draws multiple objects sequentially with a callback when complete
    /// </summary>
    /// <param name="objects">Array of GameObjects to draw</param>
    /// <param name="onComplete">Callback invoked when all objects are drawn</param>
    public void DrawMultipleObjects(GameObject[] objects, System.Action onComplete = null)
    {
        if (isAnimating)
        {
            StopCurrentAnimation();
        }

        if (objects == null || objects.Length == 0)
        {
            Debug.LogWarning("No objects to draw!");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(DrawMultipleObjectsCoroutine(objects, onComplete));
    }

    private IEnumerator DrawMultipleObjectsCoroutine(GameObject[] objects, System.Action onComplete)
    {
        isAnimating = true;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                // Draw each object
                yield return StartCoroutine(DrawingAnimationCoroutine(objects[i], true, i == objects.Length - 1));

                // Small pause between objects for visual clarity
                if (i < objects.Length - 1) // Don't pause after the last object
                    yield return new WaitForSeconds(0.2f);
            }
        }

        // Return to original position only after all drawings are complete
        yield return StartCoroutine(MoveToPosition(originalHandPosition, 0.3f));

        isAnimating = false;
        onComplete?.Invoke();
    }

    private IEnumerator DrawingAnimationCoroutine(GameObject targetObject, bool isDraw, bool returnToStart = true)
    {
        isAnimating = true;

        // Get object bounds for movement planning
        Bounds objectBounds = GetObjectBounds(targetObject);
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

        // Phase 1: Move to starting position quickly (turn off trail during movement)
        if (CurrentTrail != null) CurrentTrail.SetActive(false);
        yield return StartCoroutine(MoveToPosition(drawingPath[0], 0.3f));

        // Turn on trail for drawing
        GameObject newTrail = Instantiate(trailPrefab, CurrentTrail.transform.position, Quaternion.identity);
        newTrail.transform.SetParent(CurrentTrail.transform.parent);
        CurrentTrail.transform.SetParent(null);
        CurrentTrail = newTrail;
        if (CurrentTrail != null) CurrentTrail.SetActive(true);

        float maxSize = Math.Max(objectBounds.size.x, objectBounds.size.y);
        // Phase 2: Diagonal drawing movement with proper speed and oscillations
        float distance = Vector3.Distance(drawingPath[0], drawingPath[1]);
        float moveTime = distance / drawingSpeed; // Use proper speed calculation
        float improvedShakingIntensity = CalculateShakingIntensity(maxSize);
        yield return StartCoroutine(MoveToPositionWithOscillation(drawingPath[1], moveTime, improvedShakingIntensity));

        ApplyDrawingEffect(targetObject, isDraw);

        // Clean up trail after drawing
        if (CurrentTrail != null) CurrentTrail.SetActive(false);

        // Phase 3: Return to original position only if specified
        if (returnToStart)
        {
            yield return StartCoroutine(MoveToPosition(originalHandPosition, 0.3f));
        }

        // Stop effects
        if (drawingParticles != null)
            drawingParticles.Stop();

        isAnimating = false;
    }

    /// <summary>
    /// Calculates appropriate shaking intensity based on object size
    /// Uses a logarithmic curve to prevent excessive shaking for large objects
    /// </summary>
    private float CalculateShakingIntensity(float objectSize)
    {
        // Use a more noticeable shaking intensity that scales with object size
        return shakingIntensity * Mathf.Clamp(objectSize * 0.5f, 0.5f, 2.0f);
    }

    private Vector3[] CalculateDrawingPath(Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;

        // Create a proper diagonal movement across the entire object
        // Start from top-left corner, move to bottom-right corner
        // Ensure minimum movement distance for small objects
        float minMovement = 0.5f;
        float xOffset = Mathf.Max(size.x * 0.6f, minMovement);
        float yOffset = Mathf.Max(size.y * 0.6f, minMovement);

        Vector3 startPoint = new Vector3(
            center.x - xOffset,
            center.y + yOffset,
            center.z - drawingHeight
        );

        Vector3 endPoint = new Vector3(
            center.x + xOffset,
            center.y - yOffset,
            center.z - drawingHeight
        );

        return new Vector3[] { startPoint, endPoint };
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

    // Improved oscillating movement for better drawing effect
    private IEnumerator MoveToPositionWithOscillation(Vector3 targetPosition, float duration, float oscillationIntensity)
    {
        Vector3 startPosition = handTransform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Base diagonal movement with smooth easing
            Vector3 basePosition = Vector3.Lerp(startPosition, targetPosition, movementCurve.Evaluate(progress));

            // Add oscillation perpendicular to the movement direction
            Vector3 direction = (targetPosition - startPosition).normalized;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

            // Create natural drawing oscillations that vary in intensity
            float oscillationFreq = shakingFrequency;
            float oscillation = Mathf.Sin(elapsed * oscillationFreq) * oscillationIntensity;

            // Add some randomness for more natural hand movement
            oscillation += Mathf.PerlinNoise(elapsed * 10f, 0f) * oscillationIntensity * 0.3f;

            handTransform.position = basePosition + perpendicular * oscillation;
            yield return null;
        }

        handTransform.position = targetPosition;
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
        bool wasActive = obj.activeSelf;

        // Temporarily activate the object to get proper bounds
        if (!wasActive)
        {
            obj.SetActive(true);
        }

        Bounds bounds = new Bounds();
        bool boundsFound = false;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            bounds = renderer.bounds;
            boundsFound = true;
        }

        if (!boundsFound)
        {
            // Fallback: use collider bounds
            Collider collider = obj.GetComponent<Collider>();
            if (collider != null)
            {
                bounds = collider.bounds;
                boundsFound = true;
            }
        }

        if (!boundsFound)
        {
            // Fallback: use collider2D bounds
            Collider2D collider2D = obj.GetComponent<Collider2D>();
            if (collider2D != null)
            {
                bounds = collider2D.bounds;
                boundsFound = true;
            }
        }

        if (!boundsFound)
        {
            // Final fallback: use transform position with default size
            bounds = new Bounds(obj.transform.position, Vector3.one);
        }

        // Restore original active state
        if (!wasActive)
        {
            obj.SetActive(false);
        }

        return bounds;
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
