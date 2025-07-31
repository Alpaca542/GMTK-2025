using UnityEngine;
using UnityEngine.UI;

public class FuelManager : MonoBehaviour
{
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float currentFuel;
    [SerializeField] private float fuelConsumptionRate = 10f;

    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public float FuelPercentage => currentFuel / maxFuel;
    public bool HasFuel => currentFuel > 0;
    public Slider fuelSlider;

    void Start()
    {
        currentFuel = maxFuel;
    }

    public void CalculateFuelConsumptionBasedOnThrust(float thrust)
    {
        Debug.Log($"Calculating fuel consumption based on thrust: {thrust}");
        fuelConsumptionRate = Mathf.Clamp(thrust * 20f, 0f, 20f);
    }

    void Update()
    {
        if (HasFuel)
        {
            ConsumeFuel(fuelConsumptionRate * Time.deltaTime);
        }
        fuelSlider.value = FuelPercentage;
    }

    public void ConsumeFuel(float amount)
    {
        currentFuel = Mathf.Max(0, currentFuel - amount);
    }

    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Min(maxFuel, currentFuel + amount);
    }

    public void RefillFuel()
    {
        currentFuel = maxFuel;
    }
}
