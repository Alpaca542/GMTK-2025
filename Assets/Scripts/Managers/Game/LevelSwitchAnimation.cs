using UnityEngine;
using DG.Tweening;

public class LevelSwitchAnimation : MonoBehaviour
{
    [SerializeField] private Transform pos1;
    [SerializeField] private Transform pos2;

    public void AnimateLevelSwitch()
    {
        if (pos1 == null || pos2 == null)
        {
            Debug.LogError("pos1 or pos2 is not assigned in the inspector!");
            return;
        }

        GameObject player = GameObject.FindAnyObjectByType<PlainController>().gameObject;
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        PlainController controller = player.GetComponent<PlainController>();
        if (controller == null)
        {
            Debug.LogError("PlainController not found on player!");
            return;
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found on player!");
            return;
        }

        controller.isinanim = true;
        rb.gravityScale = 0f;
        transform.position = pos1.position;
        transform.DOMove(pos2.position, 5f)
            .SetEase(Ease.InOutSine);
    }
}