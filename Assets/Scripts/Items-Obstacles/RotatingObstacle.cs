using UnityEngine;
using DG.Tweening;

public class RotatingObstacle : MonoBehaviour
{
    [SerializeField] private Vector3[] angles;
    [SerializeField] private float speed = 90f;

    private Ease[] easeList = new Ease[]
    {
        Ease.Linear,
        Ease.InSine,
        Ease.OutSine,
        Ease.InOutSine,
        Ease.InQuad,
        Ease.OutQuad,
        Ease.InOutQuad,
        Ease.InCubic,
        Ease.OutCubic,
        Ease.InOutCubic
    };

    [SerializeField] private int easeIndex = 0;
    private int currentAngleIndex = 0;

    private void Start()
    {
        if (angles == null || angles.Length < 2)
        {
            Debug.LogError("RotatingObstacle requires at least two angles to rotate between.");
            return;
        }

        currentAngleIndex = 0;
        transform.localEulerAngles = angles[currentAngleIndex];
        RotateToNextAngle();
    }

    private void RotateToNextAngle()
    {
        int nextAngleIndex = (currentAngleIndex + 1) % angles.Length;
        Vector3 targetAngle = angles[nextAngleIndex];
        float angleDistance = Vector3.Distance(transform.localEulerAngles, targetAngle);
        float duration = angleDistance / speed;

        int clampedIndex = Mathf.Clamp(easeIndex, 0, easeList.Length - 1);
        Ease selectedEase = easeList[clampedIndex];

        transform.DOLocalRotate(targetAngle, duration)
            .SetEase(selectedEase)
            .OnComplete(() =>
            {
                currentAngleIndex = nextAngleIndex;
                RotateToNextAngle();
            });
    }
}