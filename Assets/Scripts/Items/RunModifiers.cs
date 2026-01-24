using UnityEngine;

[System.Serializable]
public class RunModifiers
{
    public int inventorySlotsBase = 5;
    public int inventorySlotsBonus = 0;

    public int shopSlotsBase = 3;
    public int shopSlotsBonus = 0;

    public float comboGainMultiplier = 1f;
    public float comboMultiplierMultiplier = 1f;

    public int InventoryMax => Mathf.Max(0, inventorySlotsBase + inventorySlotsBonus);
    public int ShopSlots => Mathf.Max(0, shopSlotsBase + shopSlotsBonus);

    public void ResetToBase()
    {
        inventorySlotsBase = 5;
        inventorySlotsBonus = 0;
        shopSlotsBase = 3;
        shopSlotsBonus = 0;
        comboGainMultiplier = 1f;
        comboMultiplierMultiplier = 1f;
    }
}
