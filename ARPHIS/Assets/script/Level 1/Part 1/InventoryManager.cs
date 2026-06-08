using System.Collections; // <--- NEW: Required for the Coroutine stopwatch!
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("UI Grid Slots")]
    public List<Image> inventorySlots = new List<Image>();
    private List<ArtifactFragment> collectedItems = new List<ArtifactFragment>();

    [Header("Drop Settings")]
    public Transform playerTransform;

    [Header("UI Popups")]
    public GameObject warningMessageUI; // <--- NEW: The slot for your PNG!

    public int GetCurrentItemCount()
    {
        return collectedItems.Count;
    }

    public bool AddItemToInventory(ArtifactFragment itemData)
    {
        // Safety Net: If full, trigger the visual warning and block the item!
        if (collectedItems.Count >= 5)
        {
            Debug.LogWarning("Inventory limit reached! Cannot add more items.");

            // Trigger the pop-up if we linked the image
            if (warningMessageUI != null)
            {
                StopAllCoroutines(); // Resets the timer if the player spams the click button
                StartCoroutine(ShowWarningRoutine());
            }

            return false;
        }

        collectedItems.Add(itemData);
        UpdateInventoryUI();
        return true;
    }

    // --- NEW: The Timer Function ---
    private IEnumerator ShowWarningRoutine()
    {
        // 1. Turn the PNG ON
        warningMessageUI.SetActive(true);

        // 2. Wait for exactly 2.5 seconds
        yield return new WaitForSeconds(2.5f);

        // 3. Turn the PNG back OFF
        warningMessageUI.SetActive(false);
    }

    public void DropItem(ArtifactFragment itemToRemove)
    {
        if (collectedItems.Contains(itemToRemove))
        {
            collectedItems.Remove(itemToRemove);

            if (playerTransform != null)
            {
                itemToRemove.transform.position = playerTransform.position + (playerTransform.forward * 1.5f) + new Vector3(0, 0.5f, 0);
            }

            itemToRemove.gameObject.SetActive(true);
            UpdateInventoryUI();
        }
    }

    private void UpdateInventoryUI()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            InventorySlotUI slotBrain = inventorySlots[i].transform.parent.GetComponent<InventorySlotUI>();

            if (i < collectedItems.Count)
            {
                inventorySlots[i].sprite = collectedItems[i].inventoryIcon;
                inventorySlots[i].enabled = true;
                slotBrain.storedFragment = collectedItems[i];
                slotBrain.hasItem = true;
            }
            else
            {
                inventorySlots[i].sprite = null;
                inventorySlots[i].enabled = false;
                slotBrain.storedFragment = null;
                slotBrain.hasItem = false;
            }
        }
    }
}