using System.Collections;
using DG.Tweening;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public GameObject audioHandler;
    public float pitchRangeDown = 0.8f;
    public float pitchRangeUp = 1.2f;

    [Header("Playback Settings")]
    public bool shouldLoop = false;
    public bool shouldPlayOnStart = false;

    private AudioSource audioSource;
    private bool isPlaying = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (shouldPlayOnStart && audioSource.clip != null)
        {
            StartPlaying(false);
        }
    }

    public void PlaySound(AudioClip clip, float pitchDown, float pitchUp, bool spawnAsChild, float volume = 1f, bool loop = false)
    {
        if (clip == null || audioHandler == null) return;

        GameObject soundObject = spawnAsChild
            ? Instantiate(audioHandler, transform.position, Quaternion.identity, transform)
            : Instantiate(audioHandler, transform.position, Quaternion.identity);

        AudioSource newSource = soundObject.GetComponent<AudioSource>();
        newSource.clip = clip;
        newSource.volume = volume;
        newSource.loop = loop;
        newSource.pitch = Random.Range(pitchDown, pitchUp);
        newSource.Play();
    }

    public void StopPlaying(bool fade = false)
    {
        if (audioSource == null) return;

        if (fade)
        {
            audioSource.DOFade(0f, 1f).OnComplete(() => audioSource.Stop());
        }
        else
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        CancelInvoke(nameof(AlterPitch));
        isPlaying = false;
    }

    public void StartPlaying(bool fade = false)
    {
        if (audioSource == null || audioSource.clip == null) return;

        if (fade)
        {
            audioSource.volume = 0f;
            audioSource.Play();
            audioSource.DOFade(0.3f, 1f);
        }
        else
        {
            audioSource.loop = shouldLoop;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        isPlaying = true;
        CancelInvoke(nameof(AlterPitch));
        InvokeRepeating(nameof(AlterPitch), 0f, audioSource.clip.length);
    }

    private void AlterPitch()
    {
        if (audioSource != null && isPlaying)
        {
            audioSource.pitch = Random.Range(pitchRangeDown, pitchRangeUp);
        }
    }
}