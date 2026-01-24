using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Helper component to handle clicks on item slots
/// Shows item info popup when clicked
/// </summary>
public class ItemSlotClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [Tooltip("The ItemSlot to show info for")]
    [SerializeField] private ItemSlot itemSlot;

    [Tooltip("The popup to show (optional, uses ItemInfoPopupManager if null)")]
    [SerializeField] private ItemInfoPopup itemInfoPopup;

    [Header("Settings")]
    [Tooltip("Show popup on click")]
    [SerializeField] private bool showPopupOnClick = true;

    [Tooltip("Mouse button to use (0=Left, 1=Right, 2=Middle)")]
    [SerializeField] private PointerEventData.InputButton clickButton = PointerEventData.InputButton.Left;

    private void Awake()
    {
        // Auto-find ItemSlot if not assigned
        if (itemSlot == null)
        {
            itemSlot = GetComponentInParent<ItemSlot>();
        }
    }

    /// <summary>
    /// Handle pointer click event
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!showPopupOnClick) return;
        if (eventData.button != clickButton) return;

        ShowItemInfo();
    }

    /// <summary>
    /// Show item info popup
    /// </summary>
    public void ShowItemInfo()
    {
        if (itemSlot == null)
        {
            Debug.LogWarning("ItemSlotClickHandler: No ItemSlot assigned!", this);
            return;
        }

        if (!itemSlot.HasItem)
        {
            Debug.Log("ItemSlotClickHandler: Slot is empty", this);
            return;
        }

        ItemData item = itemSlot.CurrentItem;

        // Use specific popup if assigned, otherwise use manager
        if (itemInfoPopup != null)
        {
            itemInfoPopup.Show(item);
        }
        else
        {
            ItemInfoPopupManager.ShowItemInfo(item);
        }
    }

    /// <summary>
    /// Set the item slot to handle
    /// </summary>
    public void SetItemSlot(ItemSlot slot)
    {
        itemSlot = slot;
    }

    /// <summary>
    /// Set the popup to use
    /// </summary>
    public void SetPopup(ItemInfoPopup popup)
    {
        itemInfoPopup = popup;
    }
}
