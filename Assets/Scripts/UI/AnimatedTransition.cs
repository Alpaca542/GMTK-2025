using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AnimatedTransition : MonoBehaviour
{
    // I hereby call this script not finished
    public float transitionDuration = 1f;
    [SerializeField] private CanvasGroup overlay;
    [SerializeField] private Image loadingImage;
    private bool loadingSmth = false;
    public static AnimatedTransition instance;

    private void Awake()
    {
        instance = this;
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        overlay.gameObject.SetActive(true);
        overlay.alpha = 1f;
        for(float t = 1f; t > 0f; t -= Time.unscaledDeltaTime * 2f)
        {
            overlay.alpha = t;
            yield return null;
        }
        overlay.alpha = 0f;
        overlay.gameObject.SetActive(false);
    }

    public void TransitionTo(string sceneName)
    {
        StartCoroutine(TransitionTo(SceneManager.GetSceneByName(sceneName)));
    }
    public void TransitionTo(int sceneIndex)
    {
        StartCoroutine(TransitionTo(SceneManager.GetSceneByBuildIndex(sceneIndex)));
    }

    // A very script from out old game MagnetMadness which adds a loadign bar to the bottom and parallel loading
    private IEnumerator TransitionTo(Scene scene)
    {
        if (loadingSmth) yield break;
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
        for (float t = 0; t <= 1; t += Time.unscaledDeltaTime * 2f)
        {
            overlay.alpha = t;
            yield return null;
        }
        overlay.alpha = 1;
        yield return new WaitForSecondsRealtime(0.1f);

        loading.allowSceneActivation = true;
        yield return new WaitUntil(() => loading.isDone);
        SceneManager.SetActiveScene(scene);
        loadingSmth = false;
        SceneManager.UnloadSceneAsync(lastScene);
    }
}
