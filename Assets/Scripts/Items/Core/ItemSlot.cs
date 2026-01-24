using UnityEngine;

/// <summary>
/// Represents a single 3D slot in the item machine
/// Displays physical item prefabs in world space
/// </summary>
public class ItemSlot : MonoBehaviour
{
    [Header("Slot Configuration")]
    [Tooltip("Prefab parent to instantiate for this slot (optional container)")]
    public GameObject slotPrefab;

    [Tooltip("Offset position for item relative to slot")]
    public Vector3 itemOffset = Vector3.zero;

    [Tooltip("Scale multiplier for displayed items")]
    public Vector3 itemScale = Vector3.one;

    [Tooltip("Rotation for displayed items")]
    public Vector3 itemRotation = Vector3.zero;

    [Header("Visualization")]
    [Tooltip("Show slot bounds in editor")]
    public bool showGizmos = true;

    [Tooltip("Gizmo color for this slot")]
    public Color gizmoColor = Color.yellow;

    [Tooltip("Size of the slot visualization")]
    public Vector3 slotSize = new Vector3(1f, 1f, 1f);

    // Current item in this slot
    private ItemData currentItem;

    // Instantiated objects
    private GameObject slotPrefabInstance;
    private GameObject itemInstance;

    /// <summary>
    /// Get the current item in this slot
    /// </summary>
    public ItemData CurrentItem => currentItem;

    /// <summary>
    /// Check if slot has an item
    /// </summary>
    public bool HasItem => currentItem != null;

    /// <summary>
    /// Get the parent transform for item spawning
    /// </summary>
    public Transform GetItemParent()
    {
        if (slotPrefabInstance != null)
        {
            return slotPrefabInstance.transform;
        }

        return transform;
    }

    private void Start()
    {
        // Create slot prefab if specified
        if (slotPrefab != null)
        {
            slotPrefabInstance = Instantiate(slotPrefab, transform);
            slotPrefabInstance.transform.localPosition = Vector3.zero;
            slotPrefabInstance.transform.localRotation = Quaternion.identity;
            slotPrefabInstance.name = $"{slotPrefab.name}_Instance";
        }
    }

    /// <summary>
    /// Set item to display in this slot
    /// </summary>
    public void SetItem(ItemData item)
    {
        // Clear previous item
        ClearSlot();

        currentItem = item;

        if (item == null || item.itemPrefab == null)
        {
            return;
        }

        // Get parent for item
        Transform parent = GetItemParent();

        // Instantiate item prefab
        itemInstance = Instantiate(item.itemPrefab, parent);

        // Apply offset, scale, and rotation
        itemInstance.transform.localPosition = itemOffset;
        itemInstance.transform.localScale = Vector3.Scale(itemInstance.transform.localScale, itemScale);
        itemInstance.transform.localRotation = Quaternion.Euler(itemRotation);

        itemInstance.name = $"{item.itemName}_Display";
    }

    /// <summary>
    /// Clear the slot
    /// </summary>
    public void ClearSlot()
    {
        currentItem = null;

        // Destroy previous item instance
        if (itemInstance != null)
        {
            if (Application.isPlaying)
            {
                Destroy(itemInstance);
            }
            else
            {
                DestroyImmediate(itemInstance);
            }

            itemInstance = null;
        }
    }

    /// <summary>
    /// Get a reference to the item and clear the slot
    /// </summary>
    public ItemData TakeItem()
    {
        ItemData item = currentItem;
        ClearSlot();
        return item;
    }

    private void OnDestroy()
    {
        ClearSlot();

        if (slotPrefabInstance != null)
        {
            if (Application.isPlaying)
            {
                Destroy(slotPrefabInstance);
            }
            else
            {
                DestroyImmediate(slotPrefabInstance);
            }
        }
    }

    /// <summary>
    /// Draw gizmos to show slot position and bounds
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw slot center
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.1f);

        // Draw slot bounds
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
        Gizmos.DrawWireCube(transform.position, slotSize);

        // Draw item offset position
        if (itemOffset != Vector3.zero)
        {
            Vector3 itemPos = transform.TransformPoint(itemOffset);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(itemPos, 0.05f);
            Gizmos.DrawLine(transform.position, itemPos);
        }
    }

    /// <summary>
    /// Draw selected gizmo with more detail
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // Draw axes
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.5f);
    }
}
