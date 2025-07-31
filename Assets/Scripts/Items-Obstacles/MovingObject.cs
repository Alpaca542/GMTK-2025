using UnityEngine;
using DG.Tweening;

public class MovingObject : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    [SerializeField] private float speed = 2f;

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
    private int currentPointIndex = 0;

    private void Start()
    {
        if (points == null || points.Length < 2)
        {
            Debug.LogError("MovingObject requires at least two points to move between.");
            return;
        }

        currentPointIndex = 0;
        transform.position = points[currentPointIndex].position;
        MoveToNextPoint();
    }

    private void MoveToNextPoint()
    {
        int nextPointIndex = (currentPointIndex + 1) % points.Length;
        Vector3 target = points[nextPointIndex].position;
        float distance = Vector3.Distance(transform.position, target);
        float duration = distance / speed;

        int clampedIndex = Mathf.Clamp(easeIndex, 0, easeList.Length - 1);
        Ease selectedEase = easeList[clampedIndex];

        transform.DOMove(target, duration)
            .SetEase(selectedEase)
            .OnComplete(() =>
            {
                currentPointIndex = nextPointIndex;
                MoveToNextPoint();
            });
    }
}
