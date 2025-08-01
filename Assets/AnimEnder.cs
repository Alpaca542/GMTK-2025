using UnityEngine;
using UnityEngine.XR;

public class AnimEnder : MonoBehaviour
{
    public HandDrawing handDrawing;
    public GameObject player;
    private void OnEnable()
    {
        Debug.Log("AnimEnder enabled");
        handDrawing.AnimateDrawing(player, true);
    }
}
