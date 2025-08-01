using UnityEngine;
using UnityEngine.XR;

public class AnimEnder : MonoBehaviour
{
    public HandDrawing handDrawing;
    public GameObject player;
    public GameObject fuel;
    private void OnEnable()
    {
        Debug.Log("AnimEnder enabled");
        handDrawing.AnimateDrawing(player, true);
        fuel.SetActive(true);
    }
}
