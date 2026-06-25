using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI coinText;
    public GameObject coinUI;

    [Header("Currency Settings")]
    public string currencySymbol = "₱";

    private int totalCoins = 0;
    private static CoinManager instance;

    void Awake()
    {
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
        // 🔥 SAVING DISABLED - Always start at 0
        totalCoins = 0;
        UpdateCoinUI();
        
        Debug.Log($"🪙 Coins started at: {totalCoins} (Saving DISABLED)");
    }

    public void AddCoins(int amount)
    {
        totalCoins += amount;
        UpdateCoinUI();
        // 🔥 SAVING DISABLED - Commented out
        // SaveCoins();
        Debug.Log($"🪙 Total coins: {totalCoins}");
    }

    public void RemoveCoins(int amount)
    {
        totalCoins = Mathf.Max(0, totalCoins - amount);
        UpdateCoinUI();
        // 🔥 SAVING DISABLED - Commented out
        // SaveCoins();
    }

    public int GetTotalCoins()
    {
        return totalCoins;
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = currencySymbol + " " + totalCoins.ToString();
        }
    }

    // 🔥 SAVING DISABLED - Kept for later use
    private void SaveCoins()
    {
        // PlayerPrefs.SetInt("TotalCoins", totalCoins);
        // PlayerPrefs.Save();
        // Debug.Log("🪙 Coins saved (DISABLED)");
    }

    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            RemoveCoins(amount);
            return true;
        }
        return false;
    }

    public void ResetCoins()
    {
        totalCoins = 0;
        UpdateCoinUI();
        // PlayerPrefs.DeleteKey("TotalCoins");
        // PlayerPrefs.Save();
        Debug.Log("🪙 Coins reset to 0");
    }

    public void ResetCoinsFromInspector()
    {
        ResetCoins();
    }
}