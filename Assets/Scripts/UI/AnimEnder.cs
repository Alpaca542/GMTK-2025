using UnityEngine;
using UnityEngine.XR;

public class AnimEnder : MonoBehaviour
{
    public HandDrawing handDrawing;
    public GameObject player;
    public GameObject fuel;
    public Animator anim;
    private void OnEnable()
    {
        handDrawing.AnimateDrawing(player, true);
        fuel.SetActive(true);
        anim.enabled = false;
    }
}
