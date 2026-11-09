using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Invent
{
public class UIEquip : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemData healthPotion;

    [Header("Potion HUD")]
    [SerializeField] private Image potionIcon;
    [SerializeField] private TMP_Text potionQuantity;

    [Header("Future Equipment HUD")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image shieldIcon;
    [SerializeField] private Image skillIcon;

    private Inventory subscribedInventory;

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
            inventory = FindFirstObjectByType<Inventory>();

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
            foreach (InventorySlot slot in inventory.slots)
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
        ItemData weapon = null;
////
        if (inventory != null)
        {
            foreach (InventorySlot slot in inventory.slots)
            {
                if (!slot.IsEmpty &&
                    slot.item.itemType == ItemType.Weapon)
                {
                    weapon = slot.item;
                    break;
                }
            }
        }

        if (weaponIcon != null)
        {
            weaponIcon.sprite = weapon != null ? weapon.icon : null;

            weaponIcon.enabled =
                weapon != null && weapon.icon != null;
        }
    }

    private void OnDisable()
    {
        if (subscribedInventory != null)
            subscribedInventory.OnInventoryChanged -= RefreshHUD;

        subscribedInventory = null;
    }
    
}
}
