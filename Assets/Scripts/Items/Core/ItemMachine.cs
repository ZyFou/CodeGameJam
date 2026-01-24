using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Machine that displays 5 random items and can drop one of them
/// Items can be rerolled to generate new options
/// </summary>
public class ItemMachine : MonoBehaviour
{
    [Header("Slot Configuration")]
    [Tooltip("The 5 item slots to display (must be exactly 5)")]
    public List<ItemSlot> itemSlots = new List<ItemSlot>();

    [Header("Spawn Settings")]
    [Tooltip("Parent transform for spawned items")]
    public Transform spawnParent;

    [Tooltip("Spawn position (local to parent)")]
    public Vector3 spawnPosition = Vector3.zero;

    [Tooltip("Use world position instead of local")]
    public bool useWorldPosition = false;

    [Header("Auto Settings")]
    [Tooltip("Automatically reroll slots on Start")]
    public bool rerollOnStart = true;

    [Tooltip("Clear slot after dropping item")]
    public bool clearSlotAfterDrop = true;

    [Tooltip("Show info popup when dropping item")]
    public bool showPopupOnDrop = true;

    [Header("Events")]
    [Tooltip("Called when slots are rerolled")]
    public UnityEvent onReroll;

    [Tooltip("Called when an item is dropped")]
    public UnityEvent<ItemData, GameObject> onItemDropped;

    [Header("Debug")]
    [Tooltip("Log machine operations")]
    public bool logOperations = true;

    [Header("Visualization")]
    [Tooltip("Show all slot zones in editor")]
    public bool showAllSlotGizmos = true;

    [Tooltip("Draw connections between slots")]
    public bool drawSlotConnections = false;

    [Tooltip("Color for machine gizmos")]
    public Color machineGizmoColor = new Color(1f, 0.5f, 0f, 0.5f);

    // Cached items currently in slots
    private List<ItemData> currentItems = new List<ItemData>();

    private void Start()
    {
        // Initialize events
        if (onReroll == null) onReroll = new UnityEvent();
        if (onItemDropped == null) onItemDropped = new UnityEvent<ItemData, GameObject>();

        // Validate slots
        if (itemSlots.Count != 5)
        {
            Debug.LogWarning($"ItemMachine: Expected 5 slots, but found {itemSlots.Count}. Please assign exactly 5 ItemSlot components.", this);
        }

        // Auto-reroll if enabled
        if (rerollOnStart)
        {
            RerollSlots();
        }
    }

    /// <summary>
    /// Reroll all 5 slots with new random items
    /// </summary>
    public void RerollSlots()
    {
        if (logOperations)
        {
            Debug.Log("ItemMachine: Rerolling all 5 slots...", this);
        }

        currentItems.Clear();

        // Generate 5 random items
        for (int i = 0; i < Mathf.Min(itemSlots.Count, 5); i++)
        {
            ItemData randomItem = ItemManager.GetRandomItem();

            if (randomItem != null)
            {
                currentItems.Add(randomItem);

                // Update slot UI
                if (itemSlots[i] != null)
                {
                    itemSlots[i].SetItem(randomItem);

                    if (logOperations)
                    {
                        Debug.Log($"  Slot {i}: {randomItem.itemName} [{randomItem.rarity}]", this);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"ItemMachine: Failed to get random item for slot {i}", this);

                if (itemSlots[i] != null)
                {
                    itemSlots[i].ClearSlot();
                }
            }
        }

        // Invoke event
        onReroll?.Invoke();

        if (logOperations)
        {
            Debug.Log("ItemMachine: Reroll complete!", this);
        }
    }

    /// <summary>
    /// Drop a random item from the 5 displayed slots
    /// </summary>
    public GameObject DropRandomFromSlots()
    {
        if (currentItems.Count == 0)
        {
            Debug.LogWarning("ItemMachine: No items in slots! Call RerollSlots() first.", this);
            return null;
        }

        // Get all non-empty slot indices
        List<int> nonEmptySlots = new List<int>();
        for (int i = 0; i < currentItems.Count; i++)
        {
            if (currentItems[i] != null)
            {
                nonEmptySlots.Add(i);
            }
        }

        if (nonEmptySlots.Count == 0)
        {
            Debug.LogWarning("ItemMachine: All slots are empty!", this);
            return null;
        }

        // Choose random non-empty slot
        int randomIndex = nonEmptySlots[Random.Range(0, nonEmptySlots.Count)];

        if (logOperations)
        {
            Debug.Log($"ItemMachine: Dropping random item from slot {randomIndex} (out of {nonEmptySlots.Count} non-empty slots)", this);
        }

        return DropSlot(randomIndex);
    }

    /// <summary>
    /// Get count of non-empty slots
    /// </summary>
    public int GetNonEmptySlotCount()
    {
        int count = 0;
        foreach (ItemData item in currentItems)
        {
            if (item != null) count++;
        }
        return count;
    }

    /// <summary>
    /// Drop the item from a specific slot
    /// </summary>
    /// <param name="slotIndex">Index of the slot (0-4)</param>
    public GameObject DropSlot(int slotIndex)
    {
        // Validate index
        if (slotIndex < 0 || slotIndex >= currentItems.Count)
        {
            Debug.LogError($"ItemMachine: Invalid slot index {slotIndex}. Valid range is 0-{currentItems.Count - 1}", this);
            return null;
        }

        ItemData itemToDrop = currentItems[slotIndex];

        if (itemToDrop == null)
        {
            Debug.LogWarning($"ItemMachine: Slot {slotIndex} is empty!", this);
            return null;
        }

        if (logOperations)
        {
            Debug.Log($"ItemMachine: Dropping '{itemToDrop.itemName}' from slot {slotIndex}", this);
        }

        // Show popup BEFORE dropping (in case slot gets cleared)
        if (showPopupOnDrop)
        {
            ItemInfoPopupManager.ShowItemInfo(itemToDrop);
        }

        // Spawn the item
        GameObject spawned;

        if (useWorldPosition)
        {
            spawned = ItemManager.SpawnItemAtWorldPosition(itemToDrop, spawnPosition, spawnParent);
        }
        else
        {
            spawned = ItemManager.SpawnItem(itemToDrop, spawnParent, spawnPosition);
        }

        if (spawned != null)
        {
            // Clear the slot if enabled
            if (clearSlotAfterDrop)
            {
                ClearSlot(slotIndex);

                if (logOperations)
                {
                    Debug.Log($"ItemMachine: Cleared slot {slotIndex}", this);
                }
            }

            // Invoke event
            onItemDropped?.Invoke(itemToDrop, spawned);

            if (logOperations)
            {
                Debug.Log($"ItemMachine: Successfully dropped '{itemToDrop.itemName}'", this);
            }
        }

        return spawned;
    }

    /// <summary>
    /// Clear a specific slot without dropping
    /// </summary>
    /// <param name="slotIndex">Index of the slot (0-4)</param>
    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentItems.Count)
        {
            Debug.LogWarning($"ItemMachine: Invalid slot index {slotIndex}", this);
            return;
        }

        if (slotIndex < itemSlots.Count && itemSlots[slotIndex] != null)
        {
            itemSlots[slotIndex].ClearSlot();
        }

        currentItems[slotIndex] = null;
    }

    /// <summary>
    /// Get the item in a specific slot without dropping it
    /// </summary>
    public ItemData GetSlotItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentItems.Count)
        {
            return null;
        }

        return currentItems[slotIndex];
    }

    /// <summary>
    /// Get all items currently in slots
    /// </summary>
    public List<ItemData> GetAllSlotItems()
    {
        return new List<ItemData>(currentItems);
    }

    /// <summary>
    /// Clear all slots
    /// </summary>
    public void ClearAllSlots()
    {
        for (int i = 0; i < currentItems.Count; i++)
        {
            currentItems[i] = null;
        }

        foreach (ItemSlot slot in itemSlots)
        {
            if (slot != null)
            {
                slot.ClearSlot();
            }
        }

        if (logOperations)
        {
            Debug.Log("ItemMachine: All slots cleared", this);
        }
    }

    /// <summary>
    /// Check if all slots are empty
    /// </summary>
    public bool AreAllSlotsEmpty()
    {
        return GetNonEmptySlotCount() == 0;
    }

    /// <summary>
    /// Check if all slots are filled
    /// </summary>
    public bool AreAllSlotsFilled()
    {
        foreach (ItemData item in currentItems)
        {
            if (item == null) return false;
        }
        return currentItems.Count > 0;
    }

    #region Button Helpers (for UI buttons)

    /// <summary>
    /// Drop slot 0 (for UI button)
    /// </summary>
    public void DropSlot0() => DropSlot(0);

    /// <summary>
    /// Drop slot 1 (for UI button)
    /// </summary>
    public void DropSlot1() => DropSlot(1);

    /// <summary>
    /// Drop slot 2 (for UI button)
    /// </summary>
    public void DropSlot2() => DropSlot(2);

    /// <summary>
    /// Drop slot 3 (for UI button)
    /// </summary>
    public void DropSlot3() => DropSlot(3);

    /// <summary>
    /// Drop slot 4 (for UI button)
    /// </summary>
    public void DropSlot4() => DropSlot(4);

    /// <summary>
    /// Show info for slot 0 (for UI button)
    /// </summary>
    public void ShowSlotInfo0() => ShowSlotInfo(0);

    /// <summary>
    /// Show info for slot 1 (for UI button)
    /// </summary>
    public void ShowSlotInfo1() => ShowSlotInfo(1);

    /// <summary>
    /// Show info for slot 2 (for UI button)
    /// </summary>
    public void ShowSlotInfo2() => ShowSlotInfo(2);

    /// <summary>
    /// Show info for slot 3 (for UI button)
    /// </summary>
    public void ShowSlotInfo3() => ShowSlotInfo(3);

    /// <summary>
    /// Show info for slot 4 (for UI button)
    /// </summary>
    public void ShowSlotInfo4() => ShowSlotInfo(4);

    /// <summary>
    /// Show item info popup for a specific slot
    /// </summary>
    public void ShowSlotInfo(int slotIndex)
    {
        ItemData item = GetSlotItem(slotIndex);
        if (item != null)
        {
            ItemInfoPopupManager.ShowItemInfo(item);
        }
        else
        {
            Debug.LogWarning($"ItemMachine: Cannot show info - slot {slotIndex} is empty!", this);
        }
    }

    #endregion

    #region Gizmos

    /// <summary>
    /// Draw gizmos to visualize all slot zones
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showAllSlotGizmos) return;

        // Draw machine bounds
        Gizmos.color = machineGizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        // Draw each slot zone
        for (int i = 0; i < itemSlots.Count; i++)
        {
            ItemSlot slot = itemSlots[i];
            if (slot == null) continue;

            // Draw line from machine to slot
            Gizmos.color = machineGizmoColor;
            Gizmos.DrawLine(transform.position, slot.transform.position);

            // Draw slot number
            #if UNITY_EDITOR
            Vector3 labelPos = slot.transform.position + Vector3.up * 0.5f;
            Handles.Label(labelPos, $"Slot {i}");
            #endif
        }

        // Draw connections between slots
        if (drawSlotConnections && itemSlots.Count > 1)
        {
            Gizmos.color = new Color(machineGizmoColor.r, machineGizmoColor.g, machineGizmoColor.b, 0.2f);

            for (int i = 0; i < itemSlots.Count - 1; i++)
            {
                if (itemSlots[i] != null && itemSlots[i + 1] != null)
                {
                    Gizmos.DrawLine(itemSlots[i].transform.position, itemSlots[i + 1].transform.position);
                }
            }
        }
    }

    /// <summary>
    /// Draw detailed gizmos when selected
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showAllSlotGizmos) return;

        // Highlight spawn position
        Vector3 spawnPos = useWorldPosition ? spawnPosition : transform.TransformPoint(spawnPosition);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnPos, 0.3f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, spawnPos);

        // Draw spawn parent connection
        if (spawnParent != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(spawnPos, spawnParent.position);
        }
    }

    #endregion

    #region Context Menu Helpers

    [ContextMenu("Test Reroll Slots")]
    private void TestReroll()
    {
        if (Application.isPlaying)
        {
            RerollSlots();
        }
        else
        {
            Debug.LogWarning("ItemMachine: Reroll only works in Play mode!");
        }
    }

    [ContextMenu("Test Drop Random")]
    private void TestDropRandom()
    {
        if (Application.isPlaying)
        {
            DropRandomFromSlots();
        }
        else
        {
            Debug.LogWarning("ItemMachine: Drop only works in Play mode!");
        }
    }

    #endregion
}
