using UnityEngine;
using UnityEngine.XR;

public class AnimEnder : MonoBehaviour
{
    public HandDrawing handDrawing;
    public GameObject player;
    public Animator anim;
    private void OnEnable()
    {
        handDrawing.AnimateDrawing(player, true);
        anim.enabled = false;
    }
}
