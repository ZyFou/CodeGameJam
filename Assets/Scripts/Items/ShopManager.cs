using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public RunManager run;
    public InventoryManager inv;

    public List<ItemDefinitionSO> pool = new List<ItemDefinitionSO>();
    public List<ItemDefinitionSO> shopSlots = new List<ItemDefinitionSO>();

    [Header("Refresh Cost")]
    public int refreshBaseCost = 5;
    public int refreshCostAddPerTour = 2;

    [Header("Optional UI")]
    public TextMeshPro[] slotTexts;
    public TextMeshPro refreshCostText;

    void Start()
    {
        RollShop();
    }

    public void RollShop()
    {
        int slotCount = GetSlotCount();

        shopSlots.Clear();
        for (int i = 0; i < slotCount; i++)
            shopSlots.Add(RollOneWeighted());

        Debug.Log($"[SHOP] Rolled {shopSlots.Count} slots.");
        RefreshUI();
    }

    public void Buy(int index)
    {
        if (run == null || inv == null)
        {
            Debug.LogWarning("[SHOP] Missing RunManager or InventoryManager.");
            return;
        }

        if (index < 0 || index >= shopSlots.Count)
        {
            Debug.LogWarning($"[SHOP] Invalid slot index {index}.");
            return;
        }

        ItemDefinitionSO item = shopSlots[index];
        if (item == null)
        {
            Debug.Log($"[SHOP] Slot {index} already sold.");
            run.RefreshHUD("Slot already sold.");
            return;
        }

        if (run.tickets <= 0)
        {
            Debug.Log("[SHOP] Not enough tickets.");
            run.RefreshHUD("Not enough tickets.");
            return;
        }

        if (inv.IsFull)
        {
            Debug.Log("[SHOP] Inventory full.");
            run.RefreshHUD("Inventory full.");
            return;
        }

        run.tickets -= 1;
        shopSlots[index] = null;

        if (!inv.TryAdd(item))
        {
            shopSlots[index] = item;
            run.tickets += 1;
            return;
        }

        Debug.Log($"[SHOP] Bought {GetItemLabel(item)} from slot {index}.");
    }

    public void RefreshShop()
    {
        if (run == null)
        {
            Debug.LogWarning("[SHOP] Missing RunManager.");
            return;
        }

        int cost = GetRefreshCost();
        if (run.money < cost)
        {
            Debug.Log("[SHOP] Not enough money to refresh.");
            run.RefreshHUD("Not enough money to refresh.");
            return;
        }

        run.money -= cost;
        Debug.Log($"[SHOP] Refresh (-{cost} money).");
        RollShop();
        run.RefreshHUD($"Shop refreshed (-{cost}).");
    }

    public int GetRefreshCost()
    {
        int tour = run != null ? Mathf.Max(1, run.tourIndex) : 1;
        return Mathf.Max(0, refreshBaseCost + (tour - 1) * refreshCostAddPerTour);
    }

    public string SlotLabel(int index)
    {
        if (index < 0 || index >= shopSlots.Count)
            return "(empty)";

        ItemDefinitionSO item = shopSlots[index];
        if (item == null)
            return "(sold)";

        return GetItemLabel(item);
    }

    public void RefreshUI()
    {
        EnsureSlotCount();

        if (slotTexts != null)
        {
            for (int i = 0; i < slotTexts.Length; i++)
            {
                if (slotTexts[i] != null)
                    slotTexts[i].text = SlotLabel(i);
            }
        }

        if (refreshCostText != null)
            refreshCostText.text = $"{GetRefreshCost()}";
    }

    void EnsureSlotCount()
    {
        int desired = GetSlotCount();
        if (desired < 0)
            desired = 0;

        if (shopSlots.Count > desired)
        {
            shopSlots.RemoveRange(desired, shopSlots.Count - desired);
            return;
        }

        while (shopSlots.Count < desired)
            shopSlots.Add(RollOneWeighted());
    }

    int GetSlotCount()
    {
        if (run != null && run.mods != null)
            return Mathf.Max(0, run.mods.ShopSlots);

        return 3;
    }

    ItemDefinitionSO RollOneWeighted()
    {
        if (pool == null || pool.Count == 0)
            return null;

        int total = 0;
        for (int i = 0; i < pool.Count; i++)
        {
            ItemDefinitionSO item = pool[i];
            if (item != null && item.weight > 0)
                total += item.weight;
        }

        if (total <= 0)
            return null;

        int roll = Random.Range(0, total);
        for (int i = 0; i < pool.Count; i++)
        {
            ItemDefinitionSO item = pool[i];
            if (item == null || item.weight <= 0)
                continue;

            if (roll < item.weight)
                return item;

            roll -= item.weight;
        }

        return null;
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
