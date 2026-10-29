
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image itemIcon;
    public TMP_Text quantityText;
    public Button button;

    private InventoryItem currentItem;
    private InventoryUI inventoryUI;

    public void Setup(InventoryItem item, InventoryUI ui)
    {
        currentItem = item;
        inventoryUI = ui;

        button.onClick.RemoveAllListeners();

        if (item == null)
        {
            itemIcon.enabled = false;
            quantityText.text = "";
            return;
        }

        itemIcon.enabled = item.item.icon != null;
        itemIcon.sprite = item.item.icon;
        quantityText.text = item.quantity > 1 ? item.quantity.ToString() : "";

        button.onClick.AddListener(SelectItem);
    }

    private void SelectItem()
    {
        inventoryUI.SelectItem(currentItem);
    }
}
