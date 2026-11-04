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

    private void Start()
    {
        if (inventory != null)
            inventory.OnInventoryChanged += RefreshHUD;

        RefreshHUD();
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
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshHUD;
    }
}
}
