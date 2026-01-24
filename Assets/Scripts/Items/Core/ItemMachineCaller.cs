using UnityEngine;

/// <summary>
/// Helper script to call ItemMachine methods from UI buttons or other scripts
/// Simply drag this onto a GameObject and reference an ItemMachine
/// </summary>
public class ItemMachineCaller : MonoBehaviour
{
    [Header("Machine Reference")]
    [Tooltip("The ItemMachine to control")]
    [SerializeField] private ItemMachine itemMachine;

    [Header("Auto Actions")]
    [Tooltip("Automatically reroll on Start")]
    [SerializeField] private bool autoRerollOnStart = false;

    [Tooltip("Automatically drop random on Start")]
    [SerializeField] private bool autoDropOnStart = false;

    [Header("Input Controls (Optional)")]
    [Tooltip("Enable keyboard controls")]
    [SerializeField] private bool enableInputControls = false;

    [Tooltip("Key to reroll slots")]
    [SerializeField] private KeyCode rerollKey = KeyCode.R;

    [Tooltip("Key to drop random item")]
    [SerializeField] private KeyCode dropRandomKey = KeyCode.Space;

    [Tooltip("Key to clear all slots")]
    [SerializeField] private KeyCode clearAllKey = KeyCode.C;

    private void Start()
    {
        if (itemMachine == null)
        {
            Debug.LogWarning("ItemMachineCaller: No ItemMachine assigned!", this);
            return;
        }

        if (autoRerollOnStart)
        {
            CallReroll();
        }

        if (autoDropOnStart)
        {
            CallDropRandom();
        }
    }

    private void Update()
    {
        if (!enableInputControls || itemMachine == null) return;

        if (Input.GetKeyDown(rerollKey))
        {
            CallReroll();
        }

        if (Input.GetKeyDown(dropRandomKey))
        {
            CallDropRandom();
        }

        if (Input.GetKeyDown(clearAllKey))
        {
            CallClearAll();
        }
    }

    /// <summary>
    /// Call reroll (for UI buttons)
    /// </summary>
    public void CallReroll()
    {
        if (itemMachine != null)
        {
            itemMachine.RerollSlots();
        }
    }

    /// <summary>
    /// Call drop random (for UI buttons)
    /// </summary>
    public void CallDropRandom()
    {
        if (itemMachine != null)
        {
            itemMachine.DropRandomFromSlots();
        }
    }

    /// <summary>
    /// Call clear all (for UI buttons)
    /// </summary>
    public void CallClearAll()
    {
        if (itemMachine != null)
        {
            itemMachine.ClearAllSlots();
        }
    }

    /// <summary>
    /// Call drop specific slot (for UI buttons)
    /// </summary>
    public void CallDropSlot(int slotIndex)
    {
        if (itemMachine != null)
        {
            itemMachine.DropSlot(slotIndex);
        }
    }

    /// <summary>
    /// Set the item machine reference from code
    /// </summary>
    public void SetItemMachine(ItemMachine machine)
    {
        itemMachine = machine;
    }
}
