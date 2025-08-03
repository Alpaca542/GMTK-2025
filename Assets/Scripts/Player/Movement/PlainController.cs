using UnityEngine;
using DG.Tweening; // Make sure you have DOTween imported
using EZCameraShake; // For camera shake effects
using System.Collections;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(Rigidbody2D))]
public class PlainController : MonoBehaviour
{
    [Header("Movement")]
    public float thrustForce = 10f;
    public float rotationSpeed = 200f;
    public float maxSpeed = 8f;
    public float baseDrag = 1f;
    public float airDrag = 3f;
    public float inputSmoothing = 8f;
    public float accelerationSmoothing = 12f;
    public float momentumDrag = 0.98f;
    public float additionalRotationSmoothing = 0.5f;

    [Header("Lift System")]
    public float liftCoefficient = 2f;
    public float minLiftSpeed = 1.0f;
    public float maxLiftSpeed = 5f;

    [Header("Wall Avoidance")]
    public float wallDetectionDistance = 1.5f;
    public float avoidanceRotationSpeed = 300f;
    public float avoidanceDuration = 0.8f;
    public float avoidanceThrust = 0.7f;
    public LayerMask wallLayer = -1;

    [Header("Cow Detection & Time Slow")]
    public float cowDetectionRadius = 2f;
    public float basketDetectionRadius = 2f;
    public float detectionCapsuleWidth = 4f;  // Width of the horizontal capsule
    public float detectionCapsuleHeight = 1f; // Height of the horizontal capsule
    public float timeSlowScale = 0.5f;
    public float timeSlowDuration = 1f;
    public float cowPickupBoostForce = 5f;
    public LayerMask cowLayer = -1;
    public LayerMask basketLayer = -1;
    [SerializeField] private Transform cowCheckTransform;
    [SerializeField] private Transform cowCheckTransformUpsideDown;
    [SerializeField] private chainHolder chainController;

    private Rigidbody2D rb;
    private float currentThrust = 0f;
    private float targetThrust = 0f;
    private Vector2 inputDirection = Vector2.zero;
    private Vector2 targetInput = Vector2.zero;
    public bool started;
    public float gravity = 4f;
    public bool isdead = false;
    public bool isinanim = false;
    public static PlainController Instance;
    private bool isGrounded = false;
    public LayerMask groundLayer;

    // Wall avoidance variables
    private bool isAvoidingWall = false;
    private float avoidanceTimer = 0f;
    private float targetAvoidanceAngle = 0f;

    // Cow detection and time slow variables
    private bool isTimeSlowed = false;
    private float timeSlowTimer = 0f;
    private float timeSlowCooldownTimer = 0f;
    private bool isOnTimeSlowCooldown = false;
    [SerializeField] private float timeSlowCooldownDuration = 1f;
    private GameObject detectedCow = null;
    private GameObject detectedBasket = null;
    private bool cowPickedUp = false;
    private bool cowDelivered = false;
    private bool isChainDeployed = false;
    private bool isCarryingBasket = false;

    // Basket positioning variables
    private Transform carriedBasket = null;

    [SerializeField] private GameObject chain;
    [SerializeField] private GameObject bound1;

    [Header("Death Effects")]
    [SerializeField] private GameObject deathParticlesPrefab; // Explosion particle effect
    [SerializeField] private float deathRespawnDelay = 2f; // Delay before respawning
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPlayer();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("MenuScene");

        if (Input.GetKey(KeyCode.Space))
        {
            if (!isChainDeployed && chainController != null)
            {
                Debug.Log("Player pressed spacebar during time slow - deploying chain");
                chainController.DeployChain();
                isChainDeployed = true;
            }
        }
        else if (isChainDeployed && chainController != null)
        {
            Debug.Log("Player released spacebar - retracting chain");
            chainController.RetractChain();
            isChainDeployed = false;
        }
        if (Input.GetKeyDown(KeyCode.F11))
            Screen.fullScreen = !Screen.fullScreen;

        // Update basket position if carrying one
        UpdateBasketPosition();
    }
    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = baseDrag;
        gravity = rb.gravityScale;

        // Subscribe to cow rescue event
        LevelManager.OnCowRescued += OnCowRescuedHandler;
    }

    void OnDestroy()
    {
        // Unsubscribe from cow rescue event
        LevelManager.OnCowRescued -= OnCowRescuedHandler;
    }

    void isGroundedCheck()
    {
        float checkRadius = 0.15f;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, checkRadius, groundLayer);
        isGrounded = hit != null;
    }

    bool DetectWallInFront()
    {
        Vector2 forwardDirection = transform.up;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, forwardDirection, wallDetectionDistance, wallLayer);

        // Debug visualization (optional - remove in production)
        Debug.DrawRay(transform.position, forwardDirection * wallDetectionDistance, hit.collider != null ? Color.red : Color.green);

        return hit.collider != null;
    }

    void StartWallAvoidance()
    {
        isAvoidingWall = true;
        avoidanceTimer = avoidanceDuration;

        // Always turn 150 degrees from current direction
        float currentAngle = transform.eulerAngles.z;
        targetAvoidanceAngle = currentAngle + 120f;

        // Normalize angle
        while (targetAvoidanceAngle >= 180f) targetAvoidanceAngle -= 360f;
        while (targetAvoidanceAngle < -180f) targetAvoidanceAngle += 360f;
    }

    void Start()
    {
        // Find chain controller if not assigned
        if (chainController == null)
        {
            chainController = FindFirstObjectByType<chainHolder>();
        }
        if (chainController == null)
        {
            Debug.LogWarning("chainHolder not found! Chain deployment/retraction will not work.");
        }

        // Validate cow check transforms
        if (cowCheckTransform == null)
        {
            Debug.LogWarning("cowCheckTransform is not assigned! Normal orientation cow detection will not work.");
        }
        if (cowCheckTransformUpsideDown == null)
        {
            Debug.LogWarning("cowCheckTransformUpsideDown is not assigned! Upside down cow detection will not work.");
        }
        if (cowCheckTransform == null && cowCheckTransformUpsideDown == null)
        {
            Debug.LogError("Both cow check transforms are missing! Cow detection will not work at all.");
        }
    }

    void FixedUpdate()
    {
        // Wall avoidance timer update
        if (isAvoidingWall)
        {
            avoidanceTimer -= Time.fixedDeltaTime;
            if (avoidanceTimer <= 0f)
            {
                isAvoidingWall = false;
                // Reset input direction immediately to prevent auto-looking upward
                inputDirection = Vector2.zero;
                targetInput = Vector2.zero;
            }
        }

        // Check for walls and start avoidance if needed
        if (!isAvoidingWall && started && DetectWallInFront())
        {
            StartWallAvoidance();
        }

        // Check for cows, baskets, and manage time slow system
        if (!isTimeSlowed && !isOnTimeSlowCooldown && started)
        {
            CheckForNearbyObjects();
        }

        // Handle time slow timer
        HandleTimeSlowSystem();

        // Handle time slow cooldown
        HandleTimeSlowCooldown();

        HandleInput();
        HandleRotation();
        HandleMovement();
        ClampVelocity();
    }


    void HandleInput()
    {
        targetInput = Vector2.zero;

        bool hasInput = false;

        isGroundedCheck();

        // Block player input during wall avoidance
        if (isAvoidingWall)
        {
            // During avoidance, maintain some input to keep thrust active
            targetInput = Vector2.up * avoidanceThrust;
            inputDirection = Vector2.Lerp(inputDirection, targetInput.normalized, inputSmoothing * Time.fixedDeltaTime);
            return;
        }

        if (isGrounded)
        {
            if (Input.GetKey(KeyCode.W)) { targetInput += Vector2.right; hasInput = true; }
            if (Input.GetKey(KeyCode.S)) { targetInput += Vector2.left; hasInput = true; }
            if (Input.GetKey(KeyCode.A)) { targetInput += Vector2.left; hasInput = true; }
            if (Input.GetKey(KeyCode.D)) { targetInput += Vector2.right; hasInput = true; }
        }
        else
        {
            if (Input.GetKey(KeyCode.W)) { targetInput += Vector2.up; hasInput = true; }
            if (Input.GetKey(KeyCode.S)) { targetInput += Vector2.down; hasInput = true; }
            if (Input.GetKey(KeyCode.A)) { targetInput += Vector2.left; hasInput = true; }
            if (Input.GetKey(KeyCode.D)) { targetInput += Vector2.right; hasInput = true; }
        }

        if (hasInput)
        {
            started = true;
        }

        inputDirection = Vector2.Lerp(inputDirection, targetInput.normalized, inputSmoothing * Time.fixedDeltaTime);
    }


    void HandleRotation()
    {
        float currentAngle = transform.eulerAngles.z;
        float targetAngle;
        float rotSpeed;

        if (isAvoidingWall)
        {
            // During wall avoidance, rotate to target avoidance angle
            targetAngle = targetAvoidanceAngle;
            rotSpeed = avoidanceRotationSpeed;
        }
        else if (inputDirection.magnitude > 0.1f)
        {
            // Normal player-controlled rotation
            targetAngle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg - 90f;
            rotSpeed = rotationSpeed;
        }
        else
        {
            return; // No rotation needed
        }

        float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
        float maxRotationThisFrame = rotSpeed * Time.fixedDeltaTime;
        float rotationThisFrame = Mathf.Clamp(angleDiff, -maxRotationThisFrame, maxRotationThisFrame);

        rb.MoveRotation(currentAngle + rotationThisFrame);
    }

    void HandleMovement()
    {
        if (!started)
        {
            rb.gravityScale = 0f;
            return;
        }

        rb.gravityScale = gravity;

        float inputMagnitude = inputDirection.magnitude;
        targetThrust = Mathf.Clamp01(inputMagnitude);
        currentThrust = Mathf.Lerp(currentThrust, targetThrust, accelerationSmoothing * Time.fixedDeltaTime);

        if (currentThrust > 0.01f)
        {
            Vector2 thrustDirection = transform.up;
            rb.AddForce(thrustForce * currentThrust * thrustDirection);
            rb.linearDamping = baseDrag;
        }
        else
        {
            rb.linearDamping = airDrag;
            rb.linearVelocity *= momentumDrag;
        }

        ApplyLift();
    }


    void ApplyLift()
    {
        float speed = rb.linearVelocity.magnitude;
        if (speed < minLiftSpeed) return;
        float liftStrength = Mathf.Clamp01(speed / maxLiftSpeed);
        Vector2 velocity = rb.linearVelocity.normalized;
        Vector2 liftDirection = new(-velocity.y, velocity.x); // Perpendicular to velocity

        // Only apply upward component of lift to prevent strange behavior
        float upwardComponent = Vector2.Dot(liftDirection, Vector2.up);

        if (upwardComponent > 0)
        {
            Vector2 liftForce = Vector2.up * liftCoefficient * liftStrength * upwardComponent;
            rb.AddForce(liftForce);
        }
    }

    void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void CheckForNearbyObjects()
    {
        MagnetScript magnetScript = FindFirstObjectByType<MagnetScript>();
        bool magnetHasCow = magnetScript != null && magnetScript.Taken;

        // Determine which cow check transform to use based on plane orientation
        Transform activeCowCheck = GetActiveCowCheckTransform();
        if (activeCowCheck == null)
        {
            Debug.LogWarning("No cow check transform available!");
            return;
        }

        // Create capsule detection area (horizontal capsule)
        // Calculate capsule rotation relative to gameobject's rotation
        // When gameobject is at -90z, capsule should be at 0 degrees (horizontal)
        // When gameobject is at 0z, capsule should be at 90 degrees
        float gameObjectRotationZ = transform.eulerAngles.z;
        float capsuleAngle = 0f;
        //gameObjectRotationZ + 90f;

        Vector2 capsuleSize = new Vector2(detectionCapsuleWidth, detectionCapsuleHeight);

        // Check for cows first (only if magnet doesn't have a cow AND not carrying a basket)
        if (!magnetHasCow && !isCarryingBasket)
        {
            Collider2D cowCollider = Physics2D.OverlapCapsule(
                activeCowCheck.position,
                capsuleSize,
                CapsuleDirection2D.Horizontal,
                capsuleAngle,
                cowLayer
            );

            if (cowCollider != null && cowCollider.CompareTag("Cow"))
            {
                Debug.Log($"Cow detected nearby with capsule: {cowCollider.name}. Starting time slow effect.");
                StartTimeSlowEffect(cowCollider.gameObject, null);
                return;
            }
        }

        // Check for baskets (only if magnet has a cow to deliver OR we need to pick up a full basket)
        if (magnetHasCow)
        {
            // Looking for basket to deliver cow to
            Collider2D basketCollider = Physics2D.OverlapCapsule(
                activeCowCheck.position,
                capsuleSize,
                CapsuleDirection2D.Horizontal,
                capsuleAngle,
                basketLayer
            );

            if (basketCollider != null && basketCollider.CompareTag("Basket"))
            {
                Debug.Log($"Basket detected nearby for cow delivery: {basketCollider.name}. Starting time slow effect.");
                StartTimeSlowEffect(null, basketCollider.gameObject);
                return;
            }
        }
        else if (!isCarryingBasket)
        {
            // Looking for full basket to pick up
            Collider2D basketCollider = Physics2D.OverlapCapsule(
                activeCowCheck.position,
                capsuleSize,
                CapsuleDirection2D.Horizontal,
                capsuleAngle,
                basketLayer
            );

            if (basketCollider != null && basketCollider.CompareTag("Basket"))
            {
                Basket basketScript = basketCollider.GetComponent<Basket>();
                if (basketScript != null && basketScript.IsPickable())
                {
                    Debug.Log($"Full basket detected nearby for pickup: {basketCollider.name}. Starting time slow effect.");
                    StartTimeSlowEffect(null, basketCollider.gameObject);
                    return;
                }
            }
        }
    }

    Transform GetActiveCowCheckTransform()
    {
        // Determine if plane is upside down based on rotation
        float currentRotation = transform.eulerAngles.z;

        // Normalize rotation to -180 to 180 range
        if (currentRotation > 180f) currentRotation -= 360f;

        // If plane is rotated more than 90 degrees (upside down), use upside down transform
        bool isUpsideDown = currentRotation >= 0f && currentRotation <= 180f;

        if (isUpsideDown && cowCheckTransformUpsideDown != null)
        {
            return cowCheckTransformUpsideDown;
        }
        else if (!isUpsideDown && cowCheckTransform != null)
        {
            return cowCheckTransform;
        }

        // Fallback to whichever transform is available
        return cowCheckTransform != null ? cowCheckTransform : cowCheckTransformUpsideDown;
    }

    void StartTimeSlowEffect(GameObject cow, GameObject basket)
    {
        if (isTimeSlowed) return;

        Debug.Log($"Starting time slow effect for {(cow != null ? "cow: " + cow.name : "basket: " + basket.name)}");
        isTimeSlowed = true;
        timeSlowTimer = timeSlowDuration;
        detectedCow = cow;
        detectedBasket = basket;
        cowPickedUp = false;
        cowDelivered = false;
        isChainDeployed = false; // Chain is NOT deployed automatically anymore

        // NOTE: Chain deployment is now controlled by spacebar during time slow
        // No automatic deployment: if (chainController != null) chainController.DeployChain();

        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, timeSlowScale, 0.2f);
    }

    public void OnCowRescuedHandler(GameObject rescuedCow)
    {
        // Check if this is the cow we're currently tracking
        if (isTimeSlowed && detectedCow == rescuedCow)
        {
            Debug.Log($"Detected cow {rescuedCow.name} was picked up! Applying boost, retracting chain, and ending time slow.");
            OnCowPickedUp();
        }
    }

    // Called when a cow is delivered to a basket
    public void OnCowDeliveredToBasket()
    {
        if (isTimeSlowed)
        {
            Debug.Log("Cow delivered to basket! Retracting chain and ending time slow.");
            cowDelivered = true;
            EndTimeSlowEffect();
        }
    }

    System.Collections.IEnumerator MonitorCowPickup()
    {
        while (isTimeSlowed && detectedCow != null)
        {
            // Check if the cow object has been destroyed (indicating it was picked up)
            if (detectedCow == null)
            {
                OnCowPickedUp();
                yield break;
            }
            yield return null;
        }
    }

    void OnCowPickedUp()
    {
        cowPickedUp = true;

        // Don't retract chain immediately - let the EndTimeSlowEffect handle it
        EndTimeSlowEffect();

        // Apply boost to the plane
        if (rb != null)
        {
            Debug.Log($"Applying cow pickup boost: {cowPickupBoostForce} force to the right");
            rb.AddForce(transform.right * cowPickupBoostForce, ForceMode2D.Impulse);
        }
    }
    void HandleTimeSlowSystem()
    {
        if (!isTimeSlowed) return;

        timeSlowTimer -= Time.unscaledDeltaTime;

        // End time slow if timer expires and no cow was picked up or delivered
        if (timeSlowTimer <= 0f && !cowPickedUp && !cowDelivered)
        {
            Debug.Log("Time slow duration expired - ending effect");
            EndTimeSlowEffect();
        }

        // Safety check: if the detected objects no longer exist, end the effect
        if (detectedCow != null && detectedCow == null) // Object was destroyed
        {
            Debug.Log("Detected cow was destroyed - ending time slow effect");
            EndTimeSlowEffect();
        }

        if (detectedBasket != null && detectedBasket == null) // Object was destroyed
        {
            Debug.Log("Detected basket was destroyed - ending time slow effect");
            EndTimeSlowEffect();
        }
    }

    void EndTimeSlowEffect()
    {
        if (!isTimeSlowed) return;

        Debug.Log("Ending time slow effect. Restoring normal time scale.");

        // Always retract chain when ending time slow effect, regardless of the reason
        if (isChainDeployed && chainController != null)
        {
            Debug.Log($"Retracting chain. CowPickedUp: {cowPickedUp}, CowDelivered: {cowDelivered}");
            chainController.RetractChain();
        }

        isTimeSlowed = false;
        timeSlowTimer = 0f;
        detectedCow = null;
        detectedBasket = null;
        cowPickedUp = false;
        cowDelivered = false;
        isChainDeployed = false;

        // Start cooldown after time slow ends
        StartTimeSlowCooldown();

        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 0.2f);
    }

    void StartTimeSlowCooldown()
    {
        isOnTimeSlowCooldown = true;
        timeSlowCooldownTimer = timeSlowCooldownDuration;
        Debug.Log($"Time slow cooldown started for {timeSlowCooldownDuration} seconds");
    }

    void HandleTimeSlowCooldown()
    {
        if (!isOnTimeSlowCooldown) return;

        timeSlowCooldownTimer -= Time.unscaledDeltaTime;

        if (timeSlowCooldownTimer <= 0f)
        {
            isOnTimeSlowCooldown = false;
            timeSlowCooldownTimer = 0f;
            Debug.Log("Time slow cooldown ended - can detect objects again");
        }
    }

    // Called when player picks up a basket
    public void OnBasketPickedUp(Transform basket)
    {
        if (basket != null)
        {
            Debug.Log($"Player picked up basket: {basket.name}");
            isCarryingBasket = true;
            carriedBasket = basket;

            // Basket is already properly attached to magnet by Basket.cs
            // No need to reparent here, just update our state
        }
    }

    // Called when basket is delivered/used
    public void OnBasketDelivered()
    {
        Debug.Log("Basket delivered - player no longer carrying basket");

        // Reset magnet state when basket is delivered
        MagnetScript magnetScript = FindFirstObjectByType<MagnetScript>();
        if (magnetScript != null)
        {
            magnetScript.Taken = false;
            Debug.Log("Magnet state reset - ready to pickup new items");
        }

        isCarryingBasket = false;
        carriedBasket = null;
    }

    void UpdateBasketPosition()
    {
        if (isCarryingBasket && carriedBasket != null)
        {
        }
    }

    public void DieBySpike()
    {
        Debug.Log("Player has died by spike - starting death sequence.");

        // Prevent multiple death calls
        if (isdead) return;

        isdead = true;
        started = false;
        rb.gravityScale = 0f;

        // Stop all movement immediately
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // End any active time slow effects
        EndTimeSlowEffect();

        // Start death sequence
        DeathSequence();
    }

    private void DeathSequence()
    {
        // 1. Spawn death particles at player position
        if (deathParticlesPrefab != null)
        {
            GameObject particles = Instantiate(deathParticlesPrefab, transform.position, transform.rotation);
            Debug.Log("Death particles spawned");
            particles.GetComponent<FadeOut>().FadeMeOut();
        }

        // 2. Add camera shake for dramatic effect
        if (CameraShaker.Instance != null)
        {
            CameraShaker.Instance.Shake(CameraShakePresets.Explosion);
            Debug.Log("Camera shake triggered");
        }

        // 4. Clean up chain state
        if (chainController != null)
        {
            chainController.RetractChain();
            chainController.ResetChainState();
        }

        // 5. Reset magnet state
        // MagnetScript magnetScript = FindFirstObjectByType<MagnetScript>();
        // if (magnetScript != null)
        // {
        //     magnetScript.Taken = false;
        //     Debug.Log("Magnet state reset after death");
        // }

        // 6. Clean up any carried baskets
        // if (isCarryingBasket && carriedBasket != null)
        // {
        //     carriedBasket.SetParent(null);
        //     isCarryingBasket = false;
        //     carriedBasket = null;
        // }

        // 8. Respawn player
        RespawnPlayer();
    }
    public void CleanUp()
    {
        if (chainController != null)
        {
            chainController.RetractChain();
            chainController.ResetChainState();
        }

        if (LevelManager.Instance != null && LevelManager.Instance.startPoint != null)
        {
            transform.position = new Vector3(
                LevelManager.Instance.startPoint.position.x,
                LevelManager.Instance.startPoint.position.y,
                -44.3f
            );
            transform.rotation = Quaternion.identity;
            chainController.transform.position = transform.position;
            Debug.Log($"Player respawned at spawn point: {transform.position}");
        }
        else
        {
            Debug.LogWarning("No spawn point found! Respawning at current position.");
        }

        ResetPlayer();
        Debug.Log("Player respawn complete");
    }
    private void RespawnPlayer()
    {
        Debug.Log("Respawning player");

        // Get spawn point from LevelManager
        if (LevelManager.Instance != null && LevelManager.Instance.startPoint != null)
        {
            transform.position = new Vector3(
                LevelManager.Instance.startPoint.position.x,
                LevelManager.Instance.startPoint.position.y,
                -44.3f
            );
            transform.rotation = Quaternion.identity;
            chainController.transform.position = transform.position;
            Debug.Log($"Player respawned at spawn point: {transform.position}");
        }
        else
        {
            Debug.LogWarning("No spawn point found! Respawning at current position.");
        }

        // Reset all player states
        ResetPlayer();
        Debug.Log("Player respawn complete");
    }
    public void ResetPlayer()
    {
        Debug.Log("Resetting player state");

        // Reset physics
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.rotation = 0f;
        rb.linearDamping = baseDrag;
        rb.gravityScale = gravity; // Restore normal gravity

        // Reset input and movement state
        inputDirection = Vector2.zero;
        targetInput = Vector2.zero;
        currentThrust = 0f;

        // Reset game state flags
        started = false;
        isdead = false;
        isinanim = false;

        // Reset wall avoidance state
        isAvoidingWall = false;
        avoidanceTimer = 0f;
        targetAvoidanceAngle = 0f;

        // Reset time slow system
        EndTimeSlowEffect();
        isChainDeployed = false;
        cowDelivered = false;
        isOnTimeSlowCooldown = false;
        timeSlowCooldownTimer = 0f;

        // Reset magnet state
        // MagnetScript magnetScript = FindFirstObjectByType<MagnetScript>();
        // if (magnetScript != null)
        // {
        //     magnetScript.Taken = false;
        // }

        // Force reset chain state
        if (chainController != null)
        {
            chainController.ResetChainState();
        }

        Debug.Log("Player reset complete");
    }

    // void DelayedReset()
    // {
    //     BackAtStart.Instance.ResetPlayerPosition(gameObject);
    // }

}
