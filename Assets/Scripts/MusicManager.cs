using DG.Tweening;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private bool ascendOnStart = true;
    [SerializeField] private float maxVolume = 0.8f;
    [SerializeField] private float fadeUpDuration = 4f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (ascendOnStart)
        {
            FadeUp();
        }
    }

    public void FadeUp()
    {
        audioSource.DOFade(maxVolume, fadeUpDuration).SetUpdate(true);
    }

    public void FadeDown(float duration)
    {
        audioSource.DOFade(0f, duration).SetUpdate(true);
    }
}