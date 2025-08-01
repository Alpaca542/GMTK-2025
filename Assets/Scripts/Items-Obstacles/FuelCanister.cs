using UnityEngine;

public class FuelCanister : MonoBehaviour
{
    public GameObject fuelParticleEffect;
    [SerializeField] private AudioClip collectionSound;
    [SerializeField] private SoundManager soundManager;
    public float fuelAmount = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (fuelParticleEffect != null)
            {
                Instantiate(fuelParticleEffect, transform.position, Quaternion.identity);
            }
            if (soundManager != null && collectionSound != null)
            {
                soundManager.PlaySound(collectionSound, 0.8f, 1.2f, false, 1f, false);
            }
            other.GetComponent<PlainController>().AddFuel(fuelAmount);
            Destroy(gameObject);
        }
    }
}