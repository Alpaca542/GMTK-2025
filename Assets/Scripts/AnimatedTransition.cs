using UnityEngine;
using DG.Tweening;

public class AnimatedTransition : MonoBehaviour
{
    public float transitionDuration = 1f;
    [SerializeField] private Vector3 beginningStartPosition;
    [SerializeField] private Vector3 endingStartPosition;

    private Transform transitionObject;

    private void Start()
    {
        transitionObject = transform;
        transitionObject.position = beginningStartPosition;
    }

    public void StartTransitionBeginning()
    {
        transitionObject.position = beginningStartPosition;
        // Transition to the screen eg beginningStartPosition is far away from the screen
    }

    public void StartTransitionEnding()
    {
        transitionObject.position = endingStartPosition;
        // Transition from the screen eg endingStartPosition is the middle of the screen
    }
}
