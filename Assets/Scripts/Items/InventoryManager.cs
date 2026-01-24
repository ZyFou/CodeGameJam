using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public RunManager run;
    public List<ItemDefinitionSO> items = new List<ItemDefinitionSO>();

    [Header("Optional UI")]
    public TextMeshPro inventoryText;

    public bool IsFull
    {
        get
        {
            if (run == null || run.mods == null)
                return true;

            return items.Count >= run.mods.InventoryMax;
        }
    }

    public bool TryAdd(ItemDefinitionSO item)
    {
        if (run == null || run.mods == null)
        {
            Debug.LogWarning("[INV] Missing RunManager/mods, cannot add item.");
            return false;
        }

        if (item == null)
        {
            Debug.LogWarning("[INV] Tried to add a null item.");
            return false;
        }

        if (IsFull)
        {
            Debug.Log($"[INV] Inventory full, cannot add {GetItemLabel(item)}.");
            run.RefreshHUD("Inventory full.");
            return false;
        }

        items.Add(item);
        item.Apply(run.mods);

        Debug.Log($"[INV] Added {GetItemLabel(item)}. Slots {items.Count}/{run.mods.InventoryMax}");
        run.RefreshHUD($"Bought {GetItemLabel(item)}.");
        run.RefreshShopUI();

        return true;
    }

    public string GetInventoryLine()
    {
        if (items.Count == 0)
            return "Inv: (empty)";

        StringBuilder sb = new StringBuilder("Inv:");
        for (int i = 0; i < items.Count; i++)
        {
            sb.Append(' ');
            sb.Append(GetItemLabel(items[i]));
        }

        return sb.ToString();
    }

    public void RefreshUI()
    {
        if (inventoryText != null)
            inventoryText.text = GetInventoryLine();
    }

    string GetItemLabel(ItemDefinitionSO item)
    {
        if (item == null)
            return "(null)";

        if (!string.IsNullOrEmpty(item.displayName))
            return item.displayName;

        return item.itemId.ToString();
    }
}
