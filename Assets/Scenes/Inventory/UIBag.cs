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
    private GridLayoutGroup slotGrid;
    private RectTransform slotContainerRect;
    private Vector2 lastSlotContainerSize;
    private int lastSlotCount = -1;

    private void Start()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

        if (slotContainer != null)
        {
            slotGrid = slotContainer.GetComponent<GridLayoutGroup>();
            slotContainerRect = slotContainer as RectTransform;
            if (slotGrid == null || slotContainerRect == null)
                Debug.LogError("Inventory slot container requires a RectTransform and GridLayoutGroup.", this);
        }

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (inventory != null)
            inventory.OnInventoryChanged += RefreshInventory;

        if (useButton != null)
            useButton.onClick.AddListener(UseSelectedItem);

        ClearSelection();
        RefreshInventory();
    }

    private void Update()
    {
        if (inventoryPanel != null && inventoryPanel.activeInHierarchy)
            FitSlotsToContainer();

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

        FitSlotsToContainer();

        if (selectedSlotIndex >= 0)
        {
            if (selectedSlotIndex >= inventory.slots.Count || inventory.slots[selectedSlotIndex].IsEmpty)
                ClearSelection();
            else
                selectedSlot = inventory.slots[selectedSlotIndex];
        }
    }

    private void FitSlotsToContainer()
    {
        if (inventory == null || slotGrid == null || slotContainerRect == null)
            return;

        Vector2 size = slotContainerRect.rect.size;
        int slotCount = inventory.slots.Count;
        if (size == lastSlotContainerSize && slotCount == lastSlotCount)
            return;

        lastSlotContainerSize = size;
        lastSlotCount = slotCount;

        int columns = Mathf.Max(1, slotGrid.constraintCount);
        int rows = Mathf.Max(1, Mathf.CeilToInt(slotCount / (float)columns));
        float availableWidth = size.x - slotGrid.padding.horizontal - slotGrid.spacing.x * (columns - 1);
        float availableHeight = size.y - slotGrid.padding.vertical - slotGrid.spacing.y * (rows - 1);
        float cellSize = Mathf.Max(0, Mathf.Min(100f, availableWidth / columns, availableHeight / rows));
        slotGrid.cellSize = new Vector2(cellSize, cellSize);
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
            useButton.interactable = slot.item.canUse &&
                slot.item.itemType == ItemType.Consumable &&
                slot.item.consumableType == ConsumableType.Shield;
    }

    private void UseSelectedItem()
    {
        if (inventory == null || selectedSlot == null || selectedSlot.IsEmpty || !selectedSlot.item.canUse)
            return;

        if (selectedSlot.item.itemType != ItemType.Consumable ||
            selectedSlot.item.consumableType != ConsumableType.Shield)
        {
            Debug.LogWarning("This item cannot be used manually.", inventory);
            return;
        }

        Test2.PlayerHealth playerHealth = inventory.GetComponent<Test2.PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("Cannot use shield potion: inventory owner has no PlayerHealth.", inventory);
            return;
        }

        if (!playerHealth.TryApplyShield(selectedSlot.item.amount))
        {
            Debug.LogWarning("Cannot use shield potion: the player is dead or the potion has no shield charges.", inventory);
            return;
        }

        Debug.Log("Used item: " + selectedSlot.item.itemName);

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

        if (useButton != null)
            useButton.onClick.RemoveListener(UseSelectedItem);
    }
}
}
