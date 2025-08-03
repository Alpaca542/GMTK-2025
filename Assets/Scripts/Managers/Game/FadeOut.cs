using UnityEngine;
using DG.Tweening;

public class FadeOut : MonoBehaviour
{
    public void FadeMeOut()
    {
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = false;
        }
        if (GetComponent<Rigidbody2D>() != null)
        {
            GetComponent<Rigidbody2D>().simulated = false;
        }
        if (GetComponent<Animator>() != null)
        {
            GetComponent<Animator>().enabled = false;
        }
        if (GetComponent<PlainController>() != null)
        {
            GetComponent<PlainController>().enabled = false;
        }
        if (GetComponent<Basket>() != null)
        {
            GetComponent<Basket>().enabled = false;
        }
        if (GetComponent<LevelEnding>() != null)
        {
            GetComponent<LevelEnding>().enabled = false;
        }
        if (gameObject.CompareTag("Text"))
        {
            Destroy(gameObject);
            return;
        }
        GetComponent<SpriteRenderer>().DOFade(0f, 1f);
        Invoke(nameof(TurnOff), 1f);
    }
    public void TurnOff()
    {
        gameObject.SetActive(false);
    }
}