using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Debug helper to validate ItemInfoPopup setup
/// </summary>
public class ItemInfoPopupDebugger : MonoBehaviour
{
    [Header("References to Check")]
    [SerializeField] private ItemInfoPopup popup;
    [SerializeField] private ItemInfoPopupManager manager;

    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        Debug.Log("=== ItemInfoPopup Setup Validation ===");

        // Check manager
        if (manager == null)
        {
            manager = FindObjectOfType<ItemInfoPopupManager>();
        }

        if (manager == null)
        {
            Debug.LogError("❌ No ItemInfoPopupManager found in scene! Please add one.");
        }
        else
        {
            Debug.Log("✅ ItemInfoPopupManager found");
        }

        // Check popup
        if (popup == null)
        {
            popup = FindObjectOfType<ItemInfoPopup>();
        }

        if (popup == null)
        {
            Debug.LogError("❌ No ItemInfoPopup found in scene! Please add one.");
            return;
        }
        else
        {
            Debug.Log("✅ ItemInfoPopup found");
        }

        // Check popup fields using reflection
        var popupType = typeof(ItemInfoPopup);

        // Check itemNameText
        var nameField = popupType.GetField("itemNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var nameText = nameField?.GetValue(popup) as TMP_Text;
        if (nameText == null)
        {
            Debug.LogError("❌ Item Name Text is not assigned in ItemInfoPopup!");
        }
        else
        {
            Debug.Log($"✅ Item Name Text assigned: {nameText.gameObject.name}");
        }

        // Check itemPriceText
        var priceField = popupType.GetField("itemPriceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var priceText = priceField?.GetValue(popup) as TMP_Text;
        if (priceText == null)
        {
            Debug.LogWarning("⚠️ Item Price Text is not assigned in ItemInfoPopup!");
        }
        else
        {
            Debug.Log($"✅ Item Price Text assigned: {priceText.gameObject.name}");
        }

        // Check itemDescriptionText
        var descField = popupType.GetField("itemDescriptionText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var descText = descField?.GetValue(popup) as TMP_Text;
        if (descText == null)
        {
            Debug.LogWarning("⚠️ Item Description Text is not assigned in ItemInfoPopup!");
        }
        else
        {
            Debug.Log($"✅ Item Description Text assigned: {descText.gameObject.name}");
        }

        // Check closeButton
        var buttonField = popupType.GetField("closeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var closeBtn = buttonField?.GetValue(popup) as Button;
        if (closeBtn == null)
        {
            Debug.LogWarning("⚠️ Close Button is not assigned in ItemInfoPopup!");
        }
        else
        {
            Debug.Log($"✅ Close Button assigned: {closeBtn.gameObject.name}");
        }

        // Check popupPanel
        var panelField = popupType.GetField("popupPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var panel = panelField?.GetValue(popup) as GameObject;
        if (panel == null)
        {
            Debug.LogWarning("⚠️ Popup Panel is not assigned - will use popup GameObject itself");
        }
        else
        {
            Debug.Log($"✅ Popup Panel assigned: {panel.name}");
            Debug.Log($"   Panel active: {panel.activeSelf}");
        }

        // Check if popup GameObject is active
        Debug.Log($"Popup GameObject active: {popup.gameObject.activeSelf}");

        Debug.Log("=== Validation Complete ===");
    }

    [ContextMenu("Test Show Random Item")]
    public void TestShowRandomItem()
    {
        if (Application.isPlaying)
        {
            ItemData randomItem = ItemManager.GetRandomItem();
            if (randomItem != null && popup != null)
            {
                Debug.Log($"Testing popup with item: {randomItem.itemName}");
                popup.Show(randomItem);
            }
            else
            {
                Debug.LogError("Cannot test - no item or no popup!");
            }
        }
        else
        {
            Debug.LogWarning("Test only works in Play mode!");
        }
    }
}
