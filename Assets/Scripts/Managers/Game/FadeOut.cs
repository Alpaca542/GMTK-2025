using UnityEngine;
using DG.Tweening;
using System;

public class FadeOut : MonoBehaviour
{
    public static event Action OnFadeOutComplete;
    private static int fadeOutsInProgress = 0;

    public void FadeMeOut()
    {
        fadeOutsInProgress++;

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
            DecrementFadeOutCounter();
            Destroy(gameObject);
            return;
        }

        if (GetComponent<SpriteRenderer>() != null)
        {
            GetComponent<SpriteRenderer>().DOFade(0f, 1f).OnComplete(() =>
            {
                TurnOff();
            });
        }
        else
        {
            // If no sprite renderer, just turn off after 1 second
            Invoke(nameof(TurnOff), 1f);
        }
    }

    public void TurnOff()
    {
        gameObject.SetActive(false);
        DecrementFadeOutCounter();
    }

    private void DecrementFadeOutCounter()
    {
        fadeOutsInProgress--;
        if (fadeOutsInProgress <= 0)
        {
            fadeOutsInProgress = 0;
            OnFadeOutComplete?.Invoke();
        }
    }

    public static void ResetFadeOutCounter()
    {
        fadeOutsInProgress = 0;
    }

    public static bool AnyFadeOutsInProgress()
    {
        return fadeOutsInProgress > 0;
    }
}