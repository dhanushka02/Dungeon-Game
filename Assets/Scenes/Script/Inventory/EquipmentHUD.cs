
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentHUD : MonoBehaviour
{
    public ItemData healthPotion;

    public Image potionIcon;
    public TMP_Text potionQuantity;

    public Image weaponIcon;
    public Image shieldIcon;
    public Image skillIcon;

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshHUD;
        }

        RefreshHUD();
    }

    public void RefreshHUD()
    {
        int quantity = 0;

        if (InventoryManager.Instance != null && healthPotion != null)
        {
            foreach (InventoryItem inventoryItem in InventoryManager.Instance.items)
            {
                if (inventoryItem.item == healthPotion)
                {
                    quantity += inventoryItem.quantity;
                }
            }
        }

        if (potionQuantity != null)
        {
            potionQuantity.text = quantity.ToString();
        }

        if (potionIcon != null)
        {
            potionIcon.sprite = healthPotion != null ? healthPotion.icon : null;
            potionIcon.enabled = quantity > 0 && healthPotion != null && healthPotion.icon != null;
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshHUD;
        }
    }
}
