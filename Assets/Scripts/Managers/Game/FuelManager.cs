using UnityEngine;
using UnityEngine.UI;

public class FuelManager : MonoBehaviour
{
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float currentFuel;
    [SerializeField] private float fuelConsumptionRate = 10f;
    [SerializeField] private float reloadCooldown = 2f;

    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public float FuelPercentage => currentFuel / maxFuel;
    public bool HasFuel => currentFuel > 0;
    public Slider fuelSlider;

    private bool isOutOfFuel = false;
    private bool isReloading = false;
    private float reloadTimer = 0f;

    void Start()
    {
        ResetFuel();
    }

    public void CalculateFuelConsumptionBasedOnThrust(float thrust)
    {
        fuelConsumptionRate = Mathf.Clamp(thrust * 20f, 0f, 20f);
    }

    void Update()
    {
        if(PlainController.Instance.isdead || PlainController.Instance.isinanim || !PlainController.Instance.started)
        {
            fuelSlider.value = FuelPercentage;
            return;
        }
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                RefillFuel();
                isReloading = false;
                isOutOfFuel = false;
            }
        }
        else if (HasFuel)
        {
            ConsumeFuel(fuelConsumptionRate * Time.deltaTime);
        }
        else if (!isOutOfFuel)
        {
            isOutOfFuel = true;
            StartReload();
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

    public void ResetFuel()
    {
        currentFuel = maxFuel;
        isOutOfFuel = false;
        isReloading = false;
        reloadTimer = 0f;
    }
    private void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadCooldown;
    }
}
