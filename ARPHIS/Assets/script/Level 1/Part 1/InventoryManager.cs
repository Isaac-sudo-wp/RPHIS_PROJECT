using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    // ==========================================
    // 🎒 CORE INVENTORY DATA
    // ==========================================
    [Header("UI Grid Slots")]
    [Tooltip("The UI Images used to display items in the player's bag.")]
    public List<Image> inventorySlots = new List<Image>();

    // 🔥 CRITICAL RULE: This list MUST ONLY contain Artifact Fragments!
    // Because this is a List of "ArtifactFragment", tools like the Chisel 
    // cannot be added here, ensuring your puzzle tray stays perfectly clean.
    public List<ArtifactFragment> collectedItems = new List<ArtifactFragment>();

    [Header("Drop Settings")]
    public Transform playerTransform;

    [Header("UI Popups")]
    public GameObject warningMessageUI;

    // ==========================================
    // 🛠️ QUEST TOOLS (KEPT SEPARATE)
    // ==========================================
    // Notice how these are just simple true/false switches, not actual items in the list.
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

    // ==========================================
    // 🔍 NPC APPRAISAL & PROGRESS CHECKS
    // ==========================================

    // Stage 1 Check: The NPC uses this to see if you have exactly 4 puzzle pieces.
    public bool HasAllFragments()
    {
        return collectedItems.Count >= 4;
    }

    // Stage 2 Check: The NPC uses this to see if you found both tools.
    public bool HasChiselAndGlue()
    {
        return hasChisel && hasWoodGlue;
    }

    // Final Check: Are all requirements met to start the puzzle?
    public bool HasAllPuzzleRequirements()
    {
        return (collectedItems.Count >= 4 && hasChisel && hasWoodGlue);
    }

    // Appraisal Check: Paeng scans the bag to make sure none of the 4 pieces are fakes.
    public bool AreAllFragmentsReal()
    {
        foreach (ArtifactFragment item in collectedItems)
        {
            if (item.isRealArtifact == false)
            {
                Debug.Log("Appraisal Failed: A fake fragment was found in the inventory!");
                return false;
            }
        }
        Debug.Log("Appraisal Passed: All fragments are 100% real!");
        return true;
    }

    public int GetCurrentItemCount()
    {
        return collectedItems.Count;
    }

    // ==========================================
    // ➕ ADDING & DROPPING ITEMS
    // ==========================================
    public bool AddItemToInventory(ArtifactFragment itemData)
    {
        // Safety Net: Block the player from picking up a 5th item!
        if (collectedItems.Count >= 4)
        {
            Debug.LogWarning("Inventory limit reached! Cannot add more items.");

            if (warningMessageUI != null)
            {
                StopAllCoroutines(); // Reset timer if they spam the click button
                StartCoroutine(ShowWarningRoutine());
            }
            return false; // Tells the street item NOT to destroy itself
        }

        // Add the item to the master list and visually update the UI
        collectedItems.Add(itemData);
        UpdateInventoryUI();
        return true; // Tells the street item it was successfully picked up
    }

    private IEnumerator ShowWarningRoutine()
    {
        warningMessageUI.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        warningMessageUI.SetActive(false);
    }

    public void DropItem(ArtifactFragment itemToRemove)
    {
        if (collectedItems.Contains(itemToRemove))
        {
            // 1. Remove it from the internal data list
            collectedItems.Remove(itemToRemove);

            // 2. Physically teleport the 3D model back to the street in front of the player
            if (playerTransform != null)
            {
                itemToRemove.transform.position = playerTransform.position + (playerTransform.forward * 1.5f) + new Vector3(0, 0.5f, 0);
            }

            // 3. Turn the 3D model back on and refresh the UI
            itemToRemove.gameObject.SetActive(true);
            UpdateInventoryUI();
        }
    }

    // ==========================================
    // 🖼️ UI SYNC LOGIC
    // ==========================================
    private void UpdateInventoryUI()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            // Grab the script sitting on the physical UI slot
            InventorySlotUI slotBrain = inventorySlots[i].transform.parent.GetComponent<InventorySlotUI>();

            if (i < collectedItems.Count)
            {
                // If we have an item for this slot, turn the picture on and feed it data!
                inventorySlots[i].sprite = collectedItems[i].inventoryIcon;
                inventorySlots[i].enabled = true;
                slotBrain.storedFragment = collectedItems[i];
                slotBrain.hasItem = true;
            }
            else
            {
                // If this slot is empty, hide the picture and clear its memory.
                inventorySlots[i].sprite = null;
                inventorySlots[i].enabled = false;
                slotBrain.storedFragment = null;
                slotBrain.hasItem = false;
            }
        }
    }
}