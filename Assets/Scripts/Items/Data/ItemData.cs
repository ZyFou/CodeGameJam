using UnityEngine;

/// <summary>
/// Item rarity levels
/// </summary>
public enum ItemRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4
}

/// <summary>
/// ScriptableObject representing a single item with all its properties
/// </summary>
[CreateAssetMenu(menuName = "Items/Item Data", fileName = "NewItem")]
public class ItemData : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("Display name of the item")]
    public string itemName;

    [Tooltip("Item description")]
    [TextArea(3, 6)]
    public string description;

    [Tooltip("Item rarity level")]
    public ItemRarity rarity = ItemRarity.Common;

    [Header("Economic Properties")]
    [Tooltip("Base purchase price")]
    [Min(0)]
    public int basePrice = 100;

    [Tooltip("Resale price (amount received when selling)")]
    [Min(0)]
    public int resalePrice = 50;

    [Header("Drop Settings")]
    [Tooltip("Drop weight/probability (higher = more likely to drop)")]
    [Min(0.01f)]
    public float dropRate = 1f;

    [Header("Prefab Reference")]
    [Tooltip("Prefab to instantiate when this item is spawned")]
    public GameObject itemPrefab;

    [Header("Visual (Optional)")]
    [Tooltip("Icon sprite for UI display")]
    public Sprite icon;

    /// <summary>
    /// Validate that the item has required data
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning($"ItemData '{name}' has no item name set!", this);
            return false;
        }

        if (itemPrefab == null)
        {
            Debug.LogWarning($"ItemData '{itemName}' has no prefab assigned!", this);
            return false;
        }

        if (dropRate <= 0)
        {
            Debug.LogWarning($"ItemData '{itemName}' has invalid drop rate!", this);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Get formatted item info for debugging
    /// </summary>
    public override string ToString()
    {
        return $"{itemName} [{rarity}] (Drop Rate: {dropRate}, Price: {basePrice}/{resalePrice})";
    }

    /// <summary>
    /// Get color associated with item rarity
    /// </summary>
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return new Color(0.7f, 0.7f, 0.7f); // Gray
            case ItemRarity.Uncommon:
                return new Color(0.2f, 1f, 0.2f); // Green
            case ItemRarity.Rare:
                return new Color(0.3f, 0.5f, 1f); // Blue
            case ItemRarity.Epic:
                return new Color(0.8f, 0.3f, 1f); // Purple
            case ItemRarity.Legendary:
                return new Color(1f, 0.6f, 0f); // Orange
            default:
                return Color.white;
        }
    }
}
