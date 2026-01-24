using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Component for dropping/spawning items in the scene
/// Can be triggered automatically or manually
/// </summary>
public class ItemDropper : MonoBehaviour
{
    [Header("Drop Configuration")]
    [Tooltip("Drop a random item on Start")]
    [SerializeField] private bool dropOnStart = false;

    [Tooltip("Specific item to drop (leave null for random)")]
    [SerializeField] private ItemData specificItem;

    [Tooltip("Use specific item instead of random")]
    [SerializeField] private bool useSpecificItem = false;

    [Header("Spawn Settings")]
    [Tooltip("Parent transform for spawned items")]
    [SerializeField] private Transform spawnParent;

    [Tooltip("Use world position instead of local position")]
    [SerializeField] private bool useWorldPosition = false;

    [Tooltip("Spawn position (world or local depending on useWorldPosition)")]
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;

    [Tooltip("Random position offset range")]
    [SerializeField] private Vector3 randomOffsetRange = Vector3.zero;

    [Header("Multiple Drops")]
    [Tooltip("Number of items to drop at once")]
    [SerializeField] [Min(1)] private int dropCount = 1;

    [Tooltip("Delay between multiple drops (seconds)")]
    [SerializeField] [Min(0)] private float dropDelay = 0f;

    [Header("Events")]
    [Tooltip("Called when an item is dropped")]
    public UnityEvent<ItemData, GameObject> onItemDropped;

    private bool hasDropped = false;

    private void Start()
    {
        if (onItemDropped == null)
        {
            onItemDropped = new UnityEvent<ItemData, GameObject>();
        }

        if (dropOnStart)
        {
            DropItem();
        }
    }

    /// <summary>
    /// Drop an item (call from buttons, events, etc.)
    /// </summary>
    public void DropItem()
    {
        if (dropCount <= 1)
        {
            DropSingleItem();
        }
        else
        {
            StartCoroutine(DropMultipleItems());
        }
    }

    /// <summary>
    /// Drop a single item immediately
    /// </summary>
    public GameObject DropSingleItem()
    {
        ItemData itemToDrop = useSpecificItem ? specificItem : ItemManager.GetRandomItem();

        if (itemToDrop == null)
        {
            Debug.LogWarning("ItemDropper: No item to drop!", this);
            return null;
        }

        // Debug parent info
        string parentInfo = spawnParent != null ? $"'{spawnParent.name}'" : "null (will use ItemManager default)";
        Debug.Log($"ItemDropper: Dropping item '{itemToDrop.itemName}' | Parent: {parentInfo} | UseWorldPos: {useWorldPosition}", this);

        // Calculate spawn position with random offset
        Vector3 finalPosition = spawnPosition + GetRandomOffset();

        GameObject spawned;

        if (useWorldPosition)
        {
            // Spawn at world position
            Debug.Log($"ItemDropper: Using world position mode | Position: {finalPosition}", this);
            spawned = ItemManager.SpawnItemAtWorldPosition(itemToDrop, finalPosition, spawnParent);
        }
        else
        {
            // Spawn with local position
            Debug.Log($"ItemDropper: Using local position mode | Local Position: {finalPosition}", this);
            spawned = ItemManager.SpawnItem(itemToDrop, spawnParent, finalPosition);
        }

        if (spawned != null)
        {
            // Verify parenting worked
            string actualParent = spawned.transform.parent != null ? spawned.transform.parent.name : "None";
            Debug.Log($"ItemDropper: Item spawned successfully | Actual Parent: {actualParent}", spawned);

            onItemDropped?.Invoke(itemToDrop, spawned);
            hasDropped = true;
        }
        else
        {
            Debug.LogError("ItemDropper: Failed to spawn item!", this);
        }

        return spawned;
    }

    /// <summary>
    /// Drop multiple items with delay
    /// </summary>
    private System.Collections.IEnumerator DropMultipleItems()
    {
        for (int i = 0; i < dropCount; i++)
        {
            DropSingleItem();

            if (i < dropCount - 1 && dropDelay > 0)
            {
                yield return new WaitForSeconds(dropDelay);
            }
        }
    }

    /// <summary>
    /// Reset the dropper so it can drop again
    /// </summary>
    public void ResetDropper()
    {
        hasDropped = false;
    }

    /// <summary>
    /// Set specific item to drop
    /// </summary>
    public void SetSpecificItem(ItemData item)
    {
        specificItem = item;
        useSpecificItem = true;
    }

    /// <summary>
    /// Set to use random items
    /// </summary>
    public void UseRandomItem()
    {
        useSpecificItem = false;
    }

    /// <summary>
    /// Set spawn parent
    /// </summary>
    public void SetSpawnParent(Transform parent)
    {
        spawnParent = parent;
    }

    /// <summary>
    /// Get random offset within range
    /// </summary>
    private Vector3 GetRandomOffset()
    {
        if (randomOffsetRange == Vector3.zero)
        {
            return Vector3.zero;
        }

        return new Vector3(
            Random.Range(-randomOffsetRange.x, randomOffsetRange.x),
            Random.Range(-randomOffsetRange.y, randomOffsetRange.y),
            Random.Range(-randomOffsetRange.z, randomOffsetRange.z)
        );
    }

    /// <summary>
    /// Visualize spawn position in editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!enabled) return;

        // Determine actual spawn position
        Vector3 gizmoPosition;

        if (useWorldPosition)
        {
            gizmoPosition = spawnPosition;
        }
        else
        {
            Transform parent = spawnParent != null ? spawnParent : transform;
            gizmoPosition = parent.TransformPoint(spawnPosition);
        }

        // Draw spawn point
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gizmoPosition, 0.2f);

        // Draw random offset range
        if (randomOffsetRange != Vector3.zero)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireCube(gizmoPosition, randomOffsetRange * 2f);
        }

        // Draw line to parent
        if (spawnParent != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(spawnParent.position, gizmoPosition);
        }
    }

    #region Context Menu Helpers

    [ContextMenu("Test Drop Item")]
    private void TestDrop()
    {
        if (Application.isPlaying)
        {
            DropItem();
        }
        else
        {
            Debug.LogWarning("ItemDropper: Test drop only works in Play mode!");
        }
    }

    [ContextMenu("Test Drop 5 Items")]
    private void TestMultipleDrop()
    {
        if (Application.isPlaying)
        {
            int originalCount = dropCount;
            dropCount = 5;
            DropItem();
            dropCount = originalCount;
        }
        else
        {
            Debug.LogWarning("ItemDropper: Test drop only works in Play mode!");
        }
    }

    #endregion
}
