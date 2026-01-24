using UnityEngine;

/// <summary>
/// Manager for showing item info popups
/// Can be called from anywhere to show item information
/// </summary>
public class ItemInfoPopupManager : MonoBehaviour
{
    [Header("Popup Reference")]
    [Tooltip("The ItemInfoPopup to control")]
    [SerializeField] private ItemInfoPopup itemInfoPopup;

    [Header("Auto Show Settings")]
    [Tooltip("Automatically show popup on specific events")]
    [SerializeField] private bool autoShowOnSlotClick = false;

    private static ItemInfoPopupManager instance;

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static ItemInfoPopupManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ItemInfoPopupManager>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        // Setup singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Validate popup reference
        if (itemInfoPopup == null)
        {
            Debug.LogWarning("ItemInfoPopupManager: No ItemInfoPopup assigned!", this);
        }
    }

    /// <summary>
    /// Show popup with item info (static method)
    /// </summary>
    public static void ShowItemInfo(ItemData item)
    {
        if (Instance == null)
        {
            Debug.LogError("ItemInfoPopupManager: No ItemInfoPopupManager instance found in scene!");
            return;
        }

        if (Instance.itemInfoPopup == null)
        {
            Debug.LogError("ItemInfoPopupManager: ItemInfoPopup is not assigned in the manager!", Instance);
            return;
        }

        Debug.Log($"ItemInfoPopupManager: Showing item info for '{item.itemName}'", Instance);
        Instance.itemInfoPopup.Show(item);
    }

    /// <summary>
    /// Show popup with item info (instance method)
    /// </summary>
    public void ShowPopup(ItemData item)
    {
        if (itemInfoPopup != null)
        {
            itemInfoPopup.Show(item);
        }
    }

    /// <summary>
    /// Close the popup (static method)
    /// </summary>
    public static void ClosePopup()
    {
        if (Instance != null && Instance.itemInfoPopup != null)
        {
            Instance.itemInfoPopup.Close();
        }
    }

    /// <summary>
    /// Close the popup (instance method)
    /// </summary>
    public void Close()
    {
        if (itemInfoPopup != null)
        {
            itemInfoPopup.Close();
        }
    }

    /// <summary>
    /// Show item info from slot (for UI buttons)
    /// </summary>
    public void ShowSlotInfo(ItemSlot slot)
    {
        if (slot != null && slot.HasItem)
        {
            ShowPopup(slot.CurrentItem);
        }
        else
        {
            Debug.LogWarning("ItemInfoPopupManager: Slot is empty or null!", this);
        }
    }

    /// <summary>
    /// Show item info from machine slot by index (for UI buttons)
    /// </summary>
    public void ShowMachineSlotInfo(ItemMachine machine, int slotIndex)
    {
        if (machine != null)
        {
            ItemData item = machine.GetSlotItem(slotIndex);
            if (item != null)
            {
                ShowPopup(item);
            }
            else
            {
                Debug.LogWarning($"ItemInfoPopupManager: Slot {slotIndex} is empty!", this);
            }
        }
    }

    #region Context Menu Helpers

    [ContextMenu("Test Show Random Item")]
    private void TestShowRandomItem()
    {
        if (Application.isPlaying)
        {
            ItemData randomItem = ItemManager.GetRandomItem();
            if (randomItem != null)
            {
                ShowPopup(randomItem);
            }
        }
        else
        {
            Debug.LogWarning("ItemInfoPopupManager: Test only works in Play mode!");
        }
    }

    [ContextMenu("Test Close Popup")]
    private void TestClose()
    {
        if (Application.isPlaying)
        {
            Close();
        }
        else
        {
            Debug.LogWarning("ItemInfoPopupManager: Test only works in Play mode!");
        }
    }

    #endregion
}
