using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class TransitionToEnding : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool hasStarted = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Transition()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            spriteRenderer.DOFade(0f, 1f).OnComplete(() => SceneManager.LoadScene("End"));
        }
    }
}
