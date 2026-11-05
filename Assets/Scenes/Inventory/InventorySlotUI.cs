using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Invent
{
public class InventorySlotUI : MonoBehaviour
{
    public Image itemIcon;
    public TMP_Text quantityText;
    public Button button;

    private InventorySlot currentSlot;
    private int currentSlotIndex;
    private UIBag inventoryUI;

    public void SetItem(ItemData item)
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = item != null ? item.icon : null;
            itemIcon.enabled = item != null && item.icon != null;
        }
    }

    public void Setup(InventorySlot slot, int slotIndex, UIBag ui)
    {
        currentSlot = slot;
        currentSlotIndex = slotIndex;
        inventoryUI = ui;

        if (button != null)
            button.onClick.RemoveAllListeners();

        if (slot == null || slot.IsEmpty)
        {
            // if (itemIcon != null)
            // {
            //     itemIcon.sprite = null;
            //     itemIcon.enabled = false;
            // }
            SetItem(null);

            if (quantityText != null)
                quantityText.text = "";

            return;
        }

        // if (itemIcon != null)
        // {
        //     itemIcon.sprite = slot.item.icon;
        //     itemIcon.enabled = slot.item.icon != null;
        // }
        SetItem(slot.item);

        if (quantityText != null)
        {
            quantityText.text =
                slot.quantity > 1
                ? slot.quantity.ToString()
                : "";
        }

        if (button != null)
            button.onClick.AddListener(SelectItem);
    }

    private void SelectItem()
    {
        if (inventoryUI == null ||
            currentSlot == null ||
            currentSlot.IsEmpty)
            return;

        inventoryUI.SelectItem(currentSlot, currentSlotIndex);
    }
}
}