using UnityEngine;

public class QuestItemPickup : MonoBehaviour
{
    public enum QuestItemType { Chisel, WoodGlue }

    [Header("What item is this?")]
    public QuestItemType itemType;

    [Header("Settings")]
    public float pickupRange = 3f; // How close the player needs to be to click it

    private Transform playerTransform;

    void Start()
    {
        // Automatically find the player when the game starts
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    // This built-in Unity function runs the exact moment you click/tap the Collider!
    private void OnMouseDown()
    {
        if (playerTransform == null) return;

        // 1. Check if the player is standing close enough
        float distance = Vector3.Distance(playerTransform.position, transform.position);

        if (distance <= pickupRange)
        {
            // 2. Search for the InventoryManager!
            InventoryManager inventory = FindObjectOfType<InventoryManager>();

            if (inventory != null)
            {
                // 3. Tell the inventory what we got
                if (itemType == QuestItemType.Chisel)
                {
                    inventory.CollectChisel();
                }
                else if (itemType == QuestItemType.WoodGlue)
                {
                    inventory.CollectWoodGlue();
                }

                // 4. Vanish!
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("You are too far away to grab that!");
        }
    }
}