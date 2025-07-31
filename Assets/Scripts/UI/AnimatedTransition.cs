using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AnimatedTransition : MonoBehaviour
{
    // I hereby call this script not finished
    [Range(0.5f, 5f)]
    public float fadeRate = 1f;
    [SerializeField] private CanvasGroup overlay;
    [SerializeField] private Image loadingImage;
    private bool loadingSmth = false;
    public static AnimatedTransition instance;

    private void Awake()
    {
        instance = this;
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        overlay.gameObject.SetActive(true);
        overlay.alpha = 1f;
        for(float t = 1f; t > 0f; t -= Time.unscaledDeltaTime * fadeRate)
        {
            overlay.alpha = t;
            yield return null;
        }
        overlay.alpha = 0f;
        overlay.gameObject.SetActive(false);
    }
    public void Exit()
    {
        StartCoroutine(exit());
    }

    private IEnumerator exit()
    {
        if (loadingSmth) yield break;
        loadingSmth = true;
        overlay.gameObject.SetActive(true);
        overlay.alpha = 0f;
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime * fadeRate)
        {
            overlay.alpha = t;
            yield return null;
        }
        overlay.alpha = 1f;
        Application.Quit();
    }

    public void TransitionTo(string sceneName)
    {
        StartCoroutine(transitionTo(sceneName));
    }
    public void TransitionTo(int sceneIndex)
    {
        StartCoroutine(transitionTo(SceneManager.GetSceneByBuildIndex(sceneIndex).name));
    }

    // A very script from out old game MagnetMadness which adds a loadign bar to the bottom and parallel loading
    private IEnumerator transitionTo(string name)
    {
        if (loadingSmth) yield break;
        StopCoroutine(FadeIn());
        loadingSmth = true;
        float progress = 0f;
        overlay.gameObject.SetActive(true);
        overlay.alpha = 0;
        yield return new WaitForSecondsRealtime(0.1f);
        while (progress < 0.05f)
        {
            progress = Mathf.Lerp(progress, 0.105f, Time.unscaledDeltaTime * 5f);
            loadingImage.fillAmount = progress;
            yield return null;
        }
        int lastScene = SceneManager.GetActiveScene().buildIndex;
        AsyncOperation loading = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        loading.allowSceneActivation = false;
        while (loading.progress < 0.9f)
        {
            if (progress < loading.progress)
            {
                progress = Mathf.Lerp(progress, loading.progress, Time.unscaledDeltaTime);
            }
            loadingImage.fillAmount = progress;
            yield return null;
        }
        while (progress < 0.98f)
        {
            progress = Mathf.Lerp(progress, 1, Time.unscaledDeltaTime * 3f);
            loadingImage.fillAmount = progress;
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.2f);
        for (float t = 0; t <= 1; t += Time.unscaledDeltaTime * 2f)
        {
            progress = Mathf.Lerp(progress, 1, Time.unscaledDeltaTime);
            loadingImage.fillAmount = progress;
            yield return null;
        }
        progress = 1f;
        loadingImage.fillAmount = progress;
        for (float t = 0; t <= 1; t += Time.unscaledDeltaTime * fadeRate)
        {
            overlay.alpha = t;
            yield return null;
        }
        overlay.alpha = 1;
        yield return new WaitForSecondsRealtime(0.3f);

        loading.allowSceneActivation = true;
        yield return new WaitUntil(() => loading.isDone);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(name));
        loadingSmth = false;
        SceneManager.UnloadSceneAsync(lastScene);
    }
}
