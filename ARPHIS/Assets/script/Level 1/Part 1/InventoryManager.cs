using System.Collections;
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
    public GameObject warningMessageUI;

    // ==========================================
    // QUEST ITEMS FOR MANLILILOK
    // ==========================================
    [Header("Quest Status")]
    public bool hasChisel = false;
    public bool hasWoodGlue = false;

    public void CollectChisel()
    {
        hasChisel = true;
        Debug.Log("Quest Item Got: Chisel!");
    }

    public void CollectWoodGlue()
    {
        hasWoodGlue = true;
        Debug.Log("Quest Item Got: Wood Glue!");
    }

    // --- NEW: Stage 1 Check (Fragments Only) ---
    // The NPC script uses this to see if you have found all 5 pieces yet.
    public bool HasAllFragments()
    {
        return collectedItems.Count >= 4;
    }

    // --- NEW: Stage 2 Check (Tools Only) ---
    // The NPC script uses this to see if you found the chisel and glue.
    public bool HasChiselAndGlue()
    {
        return hasChisel && hasWoodGlue;
    }

    // --- Original Check (Kept for safety) ---
    public bool HasAllPuzzleRequirements()
    {
        // Requires 5 fragments AND both tools all at once.
        return (collectedItems.Count >= 4 && hasChisel && hasWoodGlue);
    }
    // ==========================================

    public int GetCurrentItemCount()
    {
        return collectedItems.Count;
    }

    public bool AddItemToInventory(ArtifactFragment itemData)
    {
        // Safety Net: If full, trigger the visual warning and block the item!
        if (collectedItems.Count >= 4)
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

    // --- The Timer Function ---
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