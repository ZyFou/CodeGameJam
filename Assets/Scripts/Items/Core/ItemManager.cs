using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Singleton manager for item operations and database access
/// Provides centralized access to item data and spawning
/// </summary>
public class ItemManager : MonoBehaviour
{
    private static ItemManager instance;

    [Header("Database Reference")]
    [Tooltip("The item database to use")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("Default Spawn Settings")]
    [Tooltip("Default parent transform for spawned items (if not specified)")]
    [SerializeField] private Transform defaultSpawnParent;

    [Tooltip("Default spawn position offset")]
    [SerializeField] private Vector3 defaultSpawnOffset = Vector3.zero;

    [Header("Events")]
    [Tooltip("Called when an item is spawned")]
    public UnityEvent<ItemData, GameObject> onItemSpawned;

    [Header("Debug")]
    [Tooltip("Log item operations")]
    [SerializeField] private bool logOperations = true;

    /// <summary>
    /// Get the singleton instance
    /// </summary>
    public static ItemManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ItemManager>();

                if (instance == null)
                {
                    GameObject go = new GameObject("ItemManager");
                    instance = go.AddComponent<ItemManager>();
                }
            }

            return instance;
        }
    }

    /// <summary>
    /// Get the current item database
    /// </summary>
    public static ItemDatabase Database
    {
        get
        {
            if (Instance.itemDatabase == null)
            {
                Debug.LogError("ItemManager: No ItemDatabase assigned!");
            }

            return Instance.itemDatabase;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Validate database
        if (itemDatabase == null)
        {
            Debug.LogError("ItemManager: No ItemDatabase assigned! Please assign one in the Inspector.");
        }
        else
        {
            // Force recalculation of weights
            itemDatabase.RecalculateTotalWeight();

            if (logOperations)
            {
                Debug.Log($"ItemManager: Initialized with {itemDatabase.items.Count} items");
            }
        }

        // Initialize event if null
        if (onItemSpawned == null)
        {
            onItemSpawned = new UnityEvent<ItemData, GameObject>();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    #region Public Static API

    /// <summary>
    /// Get a random item from the database based on drop rates
    /// </summary>
    public static ItemData GetRandomItem()
    {
        if (Database == null)
        {
            return null;
        }

        return Database.GetRandomItem();
    }

    /// <summary>
    /// Get item by name
    /// </summary>
    public static ItemData GetItemByName(string itemName)
    {
        if (Database == null)
        {
            return null;
        }

        return Database.GetItemByName(itemName);
    }

    /// <summary>
    /// Spawn a random item at a specified position and parent
    /// </summary>
    /// <param name="parent">Parent transform (null = default parent)</param>
    /// <param name="localPosition">Local position relative to parent</param>
    /// <returns>The spawned GameObject or null</returns>
    public static GameObject SpawnRandomItem(Transform parent = null, Vector3? localPosition = null)
    {
        ItemData item = GetRandomItem();

        if (item == null)
        {
            Debug.LogWarning("ItemManager: Failed to get random item");
            return null;
        }

        return SpawnItem(item, parent, localPosition);
    }

    /// <summary>
    /// Spawn a specific item at a specified position and parent
    /// </summary>
    /// <param name="item">Item to spawn</param>
    /// <param name="parent">Parent transform (null = default parent)</param>
    /// <param name="localPosition">Local position relative to parent</param>
    /// <returns>The spawned GameObject or null</returns>
    public static GameObject SpawnItem(ItemData item, Transform parent = null, Vector3? localPosition = null)
    {
        if (item == null)
        {
            Debug.LogError("ItemManager: Cannot spawn null item");
            return null;
        }

        if (!item.IsValid())
        {
            Debug.LogError($"ItemManager: Cannot spawn invalid item '{item.itemName}'");
            return null;
        }

        if (item.itemPrefab == null)
        {
            Debug.LogError($"ItemManager: Item '{item.itemName}' has no prefab assigned!");
            return null;
        }

        // Determine parent
        Transform spawnParent = parent != null ? parent : Instance.defaultSpawnParent;

        // Instantiate the prefab
        GameObject spawnedObject = Instantiate(item.itemPrefab);

        // Set parent FIRST
        if (spawnParent != null)
        {
            spawnedObject.transform.SetParent(spawnParent, false);

            if (Instance.logOperations)
            {
                Debug.Log($"ItemManager: Set parent to '{spawnParent.name}'");
            }
        }
        else
        {
            if (Instance.logOperations)
            {
                Debug.Log("ItemManager: No parent set - spawning at world root");
            }
        }

        // Set position AFTER parenting
        if (localPosition.HasValue)
        {
            spawnedObject.transform.localPosition = localPosition.Value;
        }
        else
        {
            spawnedObject.transform.localPosition = Instance.defaultSpawnOffset;
        }

        // Name the spawned object
        spawnedObject.name = $"{item.itemName}_Spawned";

        // Log final result
        if (Instance.logOperations)
        {
            string parentInfo = spawnedObject.transform.parent != null
                ? $"Parent: {spawnedObject.transform.parent.name}"
                : "Parent: None (World Root)";
            Debug.Log($"ItemManager: Spawned '{item.itemName}' | {parentInfo} | World Pos: {spawnedObject.transform.position}");
        }

        // Invoke event
        Instance.onItemSpawned?.Invoke(item, spawnedObject);

        return spawnedObject;
    }

    /// <summary>
    /// Spawn item at world position
    /// </summary>
    /// <param name="item">Item to spawn</param>
    /// <param name="worldPosition">World position</param>
    /// <param name="parent">Parent transform (null = no parent)</param>
    /// <returns>The spawned GameObject or null</returns>
    public static GameObject SpawnItemAtWorldPosition(ItemData item, Vector3 worldPosition, Transform parent = null)
    {
        if (item == null || item.itemPrefab == null)
        {
            Debug.LogError("ItemManager: Cannot spawn item at world position - invalid item or prefab");
            return null;
        }

        // Instantiate the prefab
        GameObject spawnedObject = Instantiate(item.itemPrefab);

        // Set world position FIRST
        spawnedObject.transform.position = worldPosition;

        // Then set parent (preserving world position)
        if (parent != null)
        {
            spawnedObject.transform.SetParent(parent, true); // true = keep world position

            if (Instance.logOperations)
            {
                Debug.Log($"ItemManager: Parented to '{parent.name}' while keeping world position");
            }
        }

        // Name the spawned object
        spawnedObject.name = $"{item.itemName}_Spawned";

        // Log final result
        if (Instance.logOperations)
        {
            string parentInfo = spawnedObject.transform.parent != null
                ? $"Parent: {spawnedObject.transform.parent.name}"
                : "Parent: None (World Root)";
            Debug.Log($"ItemManager: Spawned '{item.itemName}' at world pos | {parentInfo} | Pos: {spawnedObject.transform.position}");
        }

        // Invoke event
        Instance.onItemSpawned?.Invoke(item, spawnedObject);

        return spawnedObject;
    }

    /// <summary>
    /// Get drop probability for an item
    /// </summary>
    public static float GetItemProbability(ItemData item)
    {
        if (Database == null)
        {
            return 0f;
        }

        return Database.GetItemProbability(item);
    }

    #endregion

    #region Context Menu Helpers

    [ContextMenu("Spawn Random Item (Test)")]
    private void TestSpawnRandomItem()
    {
        SpawnRandomItem();
    }

    [ContextMenu("Show All Item Probabilities")]
    private void ShowAllProbabilities()
    {
        if (itemDatabase != null)
        {
            foreach (ItemData item in itemDatabase.items)
            {
                if (item != null && item.IsValid())
                {
                    float prob = GetItemProbability(item);
                    Debug.Log($"{item.itemName}: {prob:F2}%");
                }
            }
        }
    }

    #endregion
}
