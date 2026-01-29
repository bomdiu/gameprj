using UnityEngine;
using UnityEngine.UI;

public class Player_Energy : MonoBehaviour
{
    public int maxEnergy; 
    public int currentEnergy;

    public Slider energyBar;

    void Start()
    {
        // --- AUTO-FIND LOGIC START ---
        if (energyBar == null)
        {
            GameObject foundObj = GameObject.Find("energybar");
            if (foundObj != null)
            {
                energyBar = foundObj.GetComponent<Slider>();
            }
        }
        // --- AUTO-FIND LOGIC END ---

        // --- LOAD DATA FROM MANAGER ---
        if (StatsManager.Instance != null)
        {
            // Load the saved values from the previous scene
            maxEnergy = StatsManager.Instance.maxEnergy;
            currentEnergy = StatsManager.Instance.currentEnergy;
        }
        else
        {
            // Default values if testing without StatsManager in the scene
            maxEnergy = 100;
            currentEnergy = 0;
        }

        // Initialize UI
        if (energyBar != null)
        {
            energyBar.minValue = 0;
            energyBar.maxValue = maxEnergy;
            energyBar.value = currentEnergy; 
        }

        UpdateUI();
    }

    public void AddEnergy(int amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
        
        // Save to Manager immediately
        if (StatsManager.Instance != null)
        {
            StatsManager.Instance.currentEnergy = currentEnergy;
        }

        UpdateUI();
    }

    public bool UseEnergy(int amount)
    {
        if (currentEnergy < amount)
            return false;

        currentEnergy -= amount;

        // Save to Manager immediately
        if (StatsManager.Instance != null)
        {
            StatsManager.Instance.currentEnergy = currentEnergy;
        }

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