using UnityEngine;
using DG.Tweening;

public class LevelSwitchAnimation : MonoBehaviour
{
    [SerializeField] private Transform pos1;
    [SerializeField] private Transform pos2;

    public void AnimateLevelSwitch()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlainController>().isinanim = true;
        player.GetComponent<Rigidbody2D>().gravityScale = 0f;
        transform.position = pos1.position;
        transform.DOMove(pos2.position, 5f)
            .SetEase(Ease.InOutSine);
    }
}