using UnityEngine;
using DG.Tweening;

public class MovingObject : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    [SerializeField] private float speed = 2f;
    public bool lookAtTarget = false;
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
    public Vector2 target2D;
    public Vector2 current2D;

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
        transform.position = new Vector3(points[currentPointIndex].position.x, points[currentPointIndex].position.y, transform.position.z);
        MoveToNextPoint();
    }

    private void MoveToNextPoint()
    {
        int nextPointIndex = (currentPointIndex + 1) % points.Length;
        target2D = new Vector2(points[nextPointIndex].position.x, points[nextPointIndex].position.y);
        current2D = new Vector2(transform.position.x, transform.position.y);
        float distance = Vector2.Distance(current2D, target2D);
        float duration = distance / speed;

        int clampedIndex = Mathf.Clamp(easeIndex, 0, easeList.Length - 1);
        Ease selectedEase = easeList[clampedIndex];

        transform.DOMove(new Vector3(target2D.x, target2D.y, transform.position.z), duration)
            .SetEase(selectedEase)
            .OnComplete(() =>
            {
                currentPointIndex = nextPointIndex;
                MoveToNextPoint();
            });
    }
    void Update()
    {
        if (lookAtTarget)
        {
            transform.right = target2D - (Vector2)transform.position;
            transform.Rotate(0, 0, 180f);
        }
    }
}
