using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject database holding all items in the game
/// Supports weighted random selection based on drop rates
/// </summary>
[CreateAssetMenu(menuName = "Items/Item Database", fileName = "ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [Header("Database Configuration")]
    [Tooltip("All items available in the game")]
    public List<ItemData> items = new List<ItemData>();

    [Header("Debug")]
    [Tooltip("Log weighted random selection details")]
    public bool logDropCalculations = false;

    // Cached total weight for optimization
    private float totalWeight = -1f;
    private bool needsRecalculation = true;

    /// <summary>
    /// Get total drop weight of all items
    /// </summary>
    public float GetTotalWeight()
    {
        if (needsRecalculation || totalWeight < 0)
        {
            RecalculateTotalWeight();
        }

        return totalWeight;
    }

    /// <summary>
    /// Recalculate total weight (call when items list changes)
    /// </summary>
    public void RecalculateTotalWeight()
    {
        totalWeight = 0f;

        foreach (ItemData item in items)
        {
            if (item != null && item.IsValid())
            {
                totalWeight += item.dropRate;
            }
        }

        needsRecalculation = false;

        if (logDropCalculations)
        {
            Debug.Log($"ItemDatabase: Total weight recalculated = {totalWeight}");
        }
    }

    /// <summary>
    /// Select a random item based on weighted drop rates
    /// </summary>
    /// <returns>Random ItemData or null if database is empty</returns>
    public ItemData GetRandomItem()
    {
        if (items == null || items.Count == 0)
        {
            Debug.LogWarning("ItemDatabase: No items in database!");
            return null;
        }

        // Ensure total weight is calculated
        float total = GetTotalWeight();

        if (total <= 0)
        {
            Debug.LogWarning("ItemDatabase: Total weight is 0! Check item drop rates.");
            return null;
        }

        // Generate random value between 0 and total weight
        float randomValue = Random.Range(0f, total);
        float currentWeight = 0f;

        if (logDropCalculations)
        {
            Debug.Log($"ItemDatabase: Random value = {randomValue} / {total}");
        }

        // Iterate through items and find which one the random value falls into
        foreach (ItemData item in items)
        {
            if (item == null || !item.IsValid())
            {
                continue;
            }

            currentWeight += item.dropRate;

            if (randomValue <= currentWeight)
            {
                if (logDropCalculations)
                {
                    float probability = (item.dropRate / total) * 100f;
                    Debug.Log($"ItemDatabase: Selected '{item.itemName}' " +
                              $"(Weight: {item.dropRate}, Probability: {probability:F2}%)");
                }

                return item;
            }
        }

        // Fallback: return last valid item (should rarely happen)
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i] != null && items[i].IsValid())
            {
                Debug.LogWarning($"ItemDatabase: Fallback to last item '{items[i].itemName}'");
                return items[i];
            }
        }

        Debug.LogError("ItemDatabase: No valid items found!");
        return null;
    }

    /// <summary>
    /// Get item by name
    /// </summary>
    public ItemData GetItemByName(string itemName)
    {
        foreach (ItemData item in items)
        {
            if (item != null && item.itemName == itemName)
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    /// Get all valid items
    /// </summary>
    public List<ItemData> GetValidItems()
    {
        List<ItemData> validItems = new List<ItemData>();

        foreach (ItemData item in items)
        {
            if (item != null && item.IsValid())
            {
                validItems.Add(item);
            }
        }

        return validItems;
    }

    /// <summary>
    /// Calculate probability percentage for an item
    /// </summary>
    public float GetItemProbability(ItemData item)
    {
        if (item == null || !item.IsValid())
        {
            return 0f;
        }

        float total = GetTotalWeight();
        if (total <= 0)
        {
            return 0f;
        }

        return (item.dropRate / total) * 100f;
    }

    /// <summary>
    /// Validate database on load
    /// </summary>
    private void OnValidate()
    {
        needsRecalculation = true;
    }

    /// <summary>
    /// Editor helper: Show drop probabilities
    /// </summary>
    [ContextMenu("Show Drop Probabilities")]
    private void ShowDropProbabilities()
    {
        RecalculateTotalWeight();

        Debug.Log("=== Item Drop Probabilities ===");
        foreach (ItemData item in items)
        {
            if (item != null && item.IsValid())
            {
                float probability = GetItemProbability(item);
                Debug.Log($"  {item.itemName}: {probability:F2}% (Weight: {item.dropRate})");
            }
        }
        Debug.Log($"Total Weight: {totalWeight}");
    }

    /// <summary>
    /// Editor helper: Test random drops
    /// </summary>
    [ContextMenu("Test 10 Random Drops")]
    private void TestRandomDrops()
    {
        Debug.Log("=== Testing 10 Random Drops ===");

        for (int i = 0; i < 10; i++)
        {
            ItemData item = GetRandomItem();
            if (item != null)
            {
                Debug.Log($"  Drop {i + 1}: {item.itemName}");
            }
        }
    }
}
