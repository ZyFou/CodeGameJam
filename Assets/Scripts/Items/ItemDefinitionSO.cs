using UnityEngine;

public enum ItemId
{
    Heart,
    Clover,
    Spade,
    Diamond
}

[CreateAssetMenu(menuName = "Booth/Item", fileName = "Item_")]
public class ItemDefinitionSO : ScriptableObject
{
    public ItemId itemId;
    public string displayName;
    public int weight = 1;

    [Header("Future (optional)")]
    public int durationRounds = 0;
    [Range(0f, 1f)]
    public float chance = 0f;

    public void Apply(RunModifiers mods)
    {
        if (mods == null)
            return;

        switch (itemId)
        {
            case ItemId.Heart:
                mods.inventorySlotsBonus += 2;
                break;
            case ItemId.Clover:
                mods.shopSlotsBonus += 1;
                break;
            case ItemId.Spade:
                mods.comboGainMultiplier *= 2f;
                break;
            case ItemId.Diamond:
                mods.comboMultiplierMultiplier *= 2f;
                break;
        }
    }
}
