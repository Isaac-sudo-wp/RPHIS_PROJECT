using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI coinText; // Drag your coin text here
    public GameObject coinUI; // Optional: the whole coin UI panel

    [Header("Currency Settings")]
    public string currencySymbol = "₱"; // Peso sign

    private int totalCoins = 0;
    private static CoinManager instance;

    void Awake()
    {
        // Singleton pattern - only one CoinManager exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Load saved coins (optional)
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        UpdateCoinUI();
    }

    public void AddCoins(int amount)
    {
        totalCoins += amount;
        UpdateCoinUI();
        SaveCoins();
        
        // Optional: Play a coin pickup sound
        Debug.Log($"🪙 Total coins: {totalCoins}");
    }

    public void RemoveCoins(int amount)
    {
        totalCoins = Mathf.Max(0, totalCoins - amount);
        UpdateCoinUI();
        SaveCoins();
    }

    public int GetTotalCoins()
    {
        return totalCoins;
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
        {
            // Display with Peso sign: "₱ 50"
            coinText.text = currencySymbol + " " + totalCoins.ToString();
        }
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();
    }

    // Call this when player wants to spend coins
    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            RemoveCoins(amount);
            return true;
        }
        return false;
    }
}