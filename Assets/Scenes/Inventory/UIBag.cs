using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Invent
{
public class UIBag : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] private Inventory inventory;

    [Header("Panels")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject equipmentHUD;

    [Header("Slot UI")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Selected Item UI")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button useButton;

    private InventorySlot selectedSlot;
    private int selectedSlotIndex = -1;

    private void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (inventory != null)
            inventory.OnInventoryChanged += RefreshInventory;

        if (useButton != null)
            useButton.onClick.AddListener(UseSelectedItem);

        RefreshInventory();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.I) || inventoryPanel == null)
            return;

        bool isOpen = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isOpen);

        if (equipmentHUD != null)
            equipmentHUD.SetActive(!isOpen);

        if (isOpen)
            RefreshInventory();
    }

    public void RefreshInventory()
    {
        if (inventory == null || slotContainer == null || slotPrefab == null)
            return;

        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < inventory.slots.Count; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, slotContainer);
            InventorySlotUI slotUI = slotObject.GetComponent<InventorySlotUI>();

            if (slotUI != null)
                slotUI.Setup(inventory.slots[i], i, this);
        }

        if (selectedSlotIndex >= 0)
        {
            if (selectedSlotIndex >= inventory.slots.Count || inventory.slots[selectedSlotIndex].IsEmpty)
                ClearSelection();
            else
                selectedSlot = inventory.slots[selectedSlotIndex];
        }
    }

    public void SelectItem(InventorySlot slot, int slotIndex)
    {
        if (slot == null || slot.IsEmpty)
            return;

        selectedSlot = slot;
        selectedSlotIndex = slotIndex;

        if (itemNameText != null)
            itemNameText.text = slot.item.itemName;

        if (descriptionText != null)
            descriptionText.text = slot.item.description;

        if (useButton != null)
            useButton.interactable = slot.item.canUse;
    }

    private void UseSelectedItem()
    {
        if (inventory == null || selectedSlot == null || selectedSlot.IsEmpty || !selectedSlot.item.canUse)
            return;

        Debug.Log("Used item: " + selectedSlot.item.itemName);

        // The actual effect (heal/equip/etc.) is intentionally not applied here yet.
        // UI/UX and gameplay effect wiring can be added after the PlayerHealth/equipment API is confirmed.
        inventory.RemoveItem(selectedSlotIndex, 1);
    }

    private void ClearSelection()
    {
        selectedSlot = null;
        selectedSlotIndex = -1;

        if (itemNameText != null)
            itemNameText.text = "";

        if (descriptionText != null)
            descriptionText.text = "";

        if (useButton != null)
            useButton.interactable = false;
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshInventory;
    }
}
}
