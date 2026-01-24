using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// UI Popup that displays item information (name, price, description)
/// Can be shown/hidden and closed with a button
/// </summary>
public class ItemInfoPopup : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text field for item name")]
    [SerializeField] private TMP_Text itemNameText;

    [Tooltip("Text field for item price")]
    [SerializeField] private TMP_Text itemPriceText;

    [Tooltip("Text field for item description")]
    [SerializeField] private TMP_Text itemDescriptionText;

    [Tooltip("Optional icon image")]
    [SerializeField] private Image itemIconImage;

    [Tooltip("Close button")]
    [SerializeField] private Button closeButton;

    [Header("Optional Components")]
    [Tooltip("CanvasGroup for fade in/out (optional)")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Tooltip("GameObject to show/hide (optional, uses this gameObject if null)")]
    [SerializeField] private GameObject popupPanel;

    [Header("Display Settings")]
    [Tooltip("Show base price or resale price")]
    [SerializeField] private bool showResalePrice = false;

    [Tooltip("Price format string (use {0} for price value)")]
    [SerializeField] private string priceFormat = "{0} Gold";

    [Tooltip("Show rarity in name")]
    [SerializeField] private bool showRarity = true;

    [Tooltip("Rarity format string (use {0} for name, {1} for rarity)")]
    [SerializeField] private string rarityFormat = "{0} [{1}]";

    [Header("Animation")]
    [Tooltip("Enable fade in/out animation")]
    [SerializeField] private bool useFadeAnimation = false;

    [Tooltip("Fade duration in seconds")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Events")]
    [Tooltip("Called when popup is opened")]
    public UnityEvent<ItemData> onPopupOpened;

    [Tooltip("Called when popup is closed")]
    public UnityEvent onPopupClosed;

    // Current item being displayed
    private ItemData currentItem;

    // Fade coroutine reference
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // Initialize events
        if (onPopupOpened == null) onPopupOpened = new UnityEvent<ItemData>();
        if (onPopupClosed == null) onPopupClosed = new UnityEvent();

        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        // Determine popup panel to use
        if (popupPanel == null)
        {
            popupPanel = gameObject;
        }

        // Start hidden
        HideImmediate();
    }

    /// <summary>
    /// Show popup with item information
    /// </summary>
    public void Show(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("ItemInfoPopup: Cannot show popup with null item!", this);
            return;
        }

        Debug.Log($"ItemInfoPopup: Showing popup for item '{item.itemName}'", this);

        currentItem = item;

        // Update UI elements
        UpdateUI();

        // Activate popup FIRST (required for coroutines)
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            Debug.Log($"ItemInfoPopup: Popup panel activated. Panel name: {popupPanel.name}", this);
        }
        else
        {
            Debug.LogWarning("ItemInfoPopup: No popup panel assigned! Using this GameObject.", this);
            gameObject.SetActive(true);
        }

        // Show popup
        if (useFadeAnimation && canvasGroup != null)
        {
            Debug.Log($"ItemInfoPopup: Using fade animation (duration: {fadeDuration}s)", this);
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeIn());
        }
        else
        {
            Debug.Log("ItemInfoPopup: Showing immediately without animation", this);
            ShowImmediate();
        }

        // Invoke event
        onPopupOpened?.Invoke(item);
    }

    /// <summary>
    /// Close the popup
    /// </summary>
    public void Close()
    {
        // Hide popup
        if (useFadeAnimation && canvasGroup != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOut());
        }
        else
        {
            HideImmediate();
        }

        // Invoke event
        onPopupClosed?.Invoke();
    }

    /// <summary>
    /// Update UI elements with current item data
    /// </summary>
    private void UpdateUI()
    {
        if (currentItem == null)
        {
            Debug.LogWarning("ItemInfoPopup: Cannot update UI - current item is null!", this);
            return;
        }

        Debug.Log($"ItemInfoPopup: Updating UI for '{currentItem.itemName}'", this);

        // Update name
        if (itemNameText != null)
        {
            if (showRarity)
            {
                itemNameText.text = string.Format(rarityFormat, currentItem.itemName, currentItem.rarity.ToString());
            }
            else
            {
                itemNameText.text = currentItem.itemName;
            }

            // Optional: Color text by rarity
            itemNameText.color = currentItem.GetRarityColor();
            Debug.Log($"ItemInfoPopup: Name text updated to '{itemNameText.text}'", this);
        }
        else
        {
            Debug.LogWarning("ItemInfoPopup: Item Name Text is not assigned!", this);
        }

        // Update price
        if (itemPriceText != null)
        {
            int price = showResalePrice ? currentItem.resalePrice : currentItem.basePrice;
            itemPriceText.text = string.Format(priceFormat, price);
            Debug.Log($"ItemInfoPopup: Price text updated to '{itemPriceText.text}'", this);
        }
        else
        {
            Debug.LogWarning("ItemInfoPopup: Item Price Text is not assigned!", this);
        }

        // Update description
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = currentItem.description;
            Debug.Log($"ItemInfoPopup: Description text updated", this);
        }
        else
        {
            Debug.LogWarning("ItemInfoPopup: Item Description Text is not assigned!", this);
        }

        // Update icon
        if (itemIconImage != null && currentItem.icon != null)
        {
            itemIconImage.sprite = currentItem.icon;
            itemIconImage.enabled = true;
            Debug.Log($"ItemInfoPopup: Icon updated", this);
        }
        else if (itemIconImage != null)
        {
            itemIconImage.enabled = false;
        }
    }

    /// <summary>
    /// Show popup immediately without animation
    /// </summary>
    private void ShowImmediate()
    {
        // PopupPanel is already activated in Show(), so no need to activate again

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    /// <summary>
    /// Hide popup immediately without animation
    /// </summary>
    private void HideImmediate()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Fade in animation
    /// </summary>
    private System.Collections.IEnumerator FadeIn()
    {
        // PopupPanel is already activated in Show(), so no need to activate again

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 0f; // Start from fully transparent

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
        }

        fadeCoroutine = null;
    }

    /// <summary>
    /// Fade out animation
    /// </summary>
    private System.Collections.IEnumerator FadeOut()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        fadeCoroutine = null;
    }

    /// <summary>
    /// Get the current item being displayed
    /// </summary>
    public ItemData GetCurrentItem()
    {
        return currentItem;
    }

    /// <summary>
    /// Check if popup is currently visible
    /// </summary>
    public bool IsVisible()
    {
        if (popupPanel != null)
        {
            return popupPanel.activeSelf;
        }
        return false;
    }

    /// <summary>
    /// Set price display mode
    /// </summary>
    public void SetShowResalePrice(bool showResale)
    {
        showResalePrice = showResale;
        if (currentItem != null)
        {
            UpdateUI();
        }
    }

    #region Context Menu Helpers

    [ContextMenu("Test Show Popup")]
    private void TestShowPopup()
    {
        if (Application.isPlaying)
        {
            ItemData testItem = ItemManager.GetRandomItem();
            if (testItem != null)
            {
                Show(testItem);
            }
            else
            {
                Debug.LogWarning("ItemInfoPopup: No item available for test!");
            }
        }
        else
        {
            Debug.LogWarning("ItemInfoPopup: Test only works in Play mode!");
        }
    }

    [ContextMenu("Test Close Popup")]
    private void TestClosePopup()
    {
        if (Application.isPlaying)
        {
            Close();
        }
        else
        {
            Debug.LogWarning("ItemInfoPopup: Test only works in Play mode!");
        }
    }

    #endregion
}
