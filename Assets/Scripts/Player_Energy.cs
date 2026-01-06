using UnityEngine;
using UnityEngine.UI;

public class Player_Energy : MonoBehaviour
{
    public int maxEnergy = 100;
    public int currentEnergy;

    public Slider energyBar;

    void Start()
    {
        currentEnergy = 0;

        if (energyBar != null)
        {
            energyBar.minValue = 0;
            energyBar.maxValue = maxEnergy;
        }

        UpdateUI();
    }

    public void AddEnergy(int amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
        UpdateUI();
    }

    public bool UseEnergy(int amount)
    {
        if (currentEnergy < amount)
            return false;

        currentEnergy -= amount;
        UpdateUI();
        return true;
    }

    void UpdateUI()
    {
        if (energyBar != null)
        {
            energyBar.value = currentEnergy;
        }
    }
}
