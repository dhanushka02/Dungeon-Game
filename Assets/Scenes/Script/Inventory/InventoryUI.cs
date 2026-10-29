
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Transform slotContainer;
    public GameObject slotPrefab;

    public TMP_Text itemNameText;
    public TMP_Text descriptionText;
    public Button useButton;

    private InventoryItem selectedItem;

    private void Start()
    {
        inventoryPanel.SetActive(false);

        InventoryManager.Instance.OnInventoryChanged += RefreshInventory;

        useButton.onClick.AddListener(UseSelectedItem);
        RefreshInventory();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);

            if (inventoryPanel.activeSelf)
                RefreshInventory();
        }
    }

    public void RefreshInventory()
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        foreach (InventoryItem item in InventoryManager.Instance.items)
        {
            GameObject slot = Instantiate(slotPrefab, slotContainer);
            slot.GetComponent<InventorySlotUI>().Setup(item, this);
        }

        int emptySlots = InventoryManager.Instance.maxSlots -
                         InventoryManager.Instance.items.Count;

        for (int i = 0; i < emptySlots; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotContainer);
            slot.GetComponent<InventorySlotUI>().Setup(null, this);
        }

        if (selectedItem != null &&
            !InventoryManager.Instance.items.Contains(selectedItem))
        {
            selectedItem = null;
            itemNameText.text = "";
            descriptionText.text = "";
            useButton.interactable = false;
        }
    }

    public void SelectItem(InventoryItem item)
    {
        selectedItem = item;

        itemNameText.text = item.item.itemName;
        descriptionText.text = item.item.description;
        useButton.interactable = item.item.canUse;
    }

    private void UseSelectedItem()
    {
        if (selectedItem == null || !selectedItem.item.canUse)
            return;

        ItemData item = selectedItem.item;

        Debug.Log("Used item: " + item.itemName);

        InventoryManager.Instance.RemoveItem(item);
        RefreshInventory();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;
    }
}
