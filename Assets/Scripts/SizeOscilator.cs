using UnityEngine;

using DG.Tweening;

public class SizeOscilator : MonoBehaviour
{
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private float duration = 1f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        StartOscillation();
    }

    private void StartOscillation()
    {
        transform.localScale = originalScale * minScale;

        transform.DOScale(originalScale * maxScale, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
