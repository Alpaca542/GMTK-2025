using UnityEngine;

public class DieInTime : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;

    void Start()
    {
        Invoke(nameof(DestroySelf), lifeTime);
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
