
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentHUD : MonoBehaviour
{
    public Invent.ItemData healthPotion;

    public Image potionIcon;
    public TMP_Text potionQuantity;

    public Image weaponIcon;
    public Image shieldIcon;
    public Image skillIcon;

    [SerializeField] private Invent.Inventory inventory;
    private Invent.Inventory subscribedInventory;

    private void OnEnable()
    {
        BindInventory();
        RefreshHUD();
    }

    private void Start()
    {
        BindInventory();
        RefreshHUD();
    }

    private void BindInventory()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<Invent.Inventory>();

        if (subscribedInventory == inventory)
            return;

        if (subscribedInventory != null)
            subscribedInventory.OnInventoryChanged -= RefreshHUD;

        subscribedInventory = inventory;
        if (subscribedInventory != null)
            subscribedInventory.OnInventoryChanged += RefreshHUD;
    }

    public void RefreshHUD()
    {
        int quantity = 0;

        if (inventory != null && healthPotion != null)
        {
            foreach (Invent.InventorySlot slot in inventory.slots)
            {
                if (!slot.IsEmpty && slot.item == healthPotion)
                    quantity += slot.quantity;
            }
        }

        if (potionQuantity != null)
            potionQuantity.text = quantity.ToString();

        if (potionIcon != null)
        {
            potionIcon.sprite = healthPotion != null ? healthPotion.icon : null;
            potionIcon.enabled = quantity > 0 && healthPotion != null && healthPotion.icon != null;
        }
    }

    private void OnDisable()
    {
        if (subscribedInventory != null)
            subscribedInventory.OnInventoryChanged -= RefreshHUD;

        subscribedInventory = null;
    }
}
