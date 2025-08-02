using UnityEngine;

public class chainHolder : MonoBehaviour
{
    public Rigidbody2D planeRb;
    public MagnetScript magnetScript;
    public SpringJoint2D magnetSpringJoint;
    public bool isChainDeployed = false;
    public GameObject[] cainObjects;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Find magnet script and spring joint if not assigned
        if (magnetScript == null)
        {
            magnetScript = FindFirstObjectByType<MagnetScript>();
            if (magnetScript == null)
            {
                Debug.LogError("MagnetScript not found! Chain system will not work properly.");
            }
        }

        if (magnetSpringJoint == null && magnetScript != null)
        {
            magnetSpringJoint = magnetScript.GetComponent<SpringJoint2D>();
            if (magnetSpringJoint == null)
            {
                Debug.LogError("SpringJoint2D not found on magnet! Chain retraction will not work.");
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 targetPos = planeRb.position;
        rb.MovePosition(targetPos);
    }

    public void DeployChain()
    {
        if (magnetSpringJoint != null)
        {
            setPiecesOn(true);
            isChainDeployed = true;
            magnetSpringJoint.enabled = false;
            Debug.Log("Chain deployed - Spring joint disabled");
        }
        else
        {
            Debug.LogWarning("Cannot deploy chain - SpringJoint2D is null");
        }
    }
    public void setPiecesOn(bool value)
    {
        foreach (GameObject obj in cainObjects)
        {
            if (obj != null)
            {
                obj.GetComponent<SpriteRenderer>().enabled = value;
            }
        }
    }
    public void RetractChain()
    {
        if (magnetSpringJoint != null)
        {
            Invoke(nameof(setPiecesOff), 0.3f);
            isChainDeployed = false;
            magnetSpringJoint.enabled = true;
            Debug.Log("Chain retracted - Spring joint enabled");
        }
        else
        {
            Debug.LogWarning("Cannot retract chain - SpringJoint2D is null");
        }
    }

    private void setPiecesOff()
    {
        setPiecesOn(false);
    }

    public bool CanRetractChain()
    {
        return magnetScript != null && !magnetScript.Taken;
    }

    public bool IsChainDeployed()
    {
        return magnetSpringJoint != null && !magnetSpringJoint.enabled;
    }

    public bool IsChainRetracted()
    {
        return magnetSpringJoint != null && magnetSpringJoint.enabled;
    }

    public void ForceRetractChain()
    {
        if (magnetSpringJoint != null)
        {
            magnetSpringJoint.enabled = true;
            Debug.Log("Chain force retracted - Spring joint enabled (ignoring magnet state)");
        }
    }

    public void ResetChainState()
    {
        if (magnetSpringJoint != null)
        {
            magnetSpringJoint.enabled = true;
            Debug.Log("Chain state reset - Spring joint enabled");
        }
        if (magnetScript != null)
        {
            magnetScript.Taken = false;
            Debug.Log("Magnet state reset - Taken = false");
        }
    }
}
