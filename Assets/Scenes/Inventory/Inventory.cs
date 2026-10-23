using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 20;

    public List<InventorySlot> slots = new List<InventorySlot>();

    public event Action OnInventoryChanged;

    private void Awake()
    {
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        slots.Clear();

        for (int i = 0; i < inventorySize; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        int remaining = amount;

        if (item.stackable)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.item == item &&
                    slot.quantity < item.maxStack)
                {
                    int freeSpace =
                        item.maxStack - slot.quantity;

                    int amountToAdd =
                        Mathf.Min(freeSpace, remaining);

                    slot.quantity += amountToAdd;
                    remaining -= amountToAdd;

                    if (remaining <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty)
                continue;

            int amountToAdd;

            if (item.stackable)
                amountToAdd =
                    Mathf.Min(item.maxStack, remaining);
            else
                amountToAdd = 1;

            slot.item = item;
            slot.quantity = amountToAdd;

            remaining -= amountToAdd;

            if (remaining <= 0)
            {
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        OnInventoryChanged?.Invoke();

        return false;
    }

    public void RemoveItem(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
            return;

        InventorySlot slot = slots[slotIndex];

        if (slot.IsEmpty)
            return;

        slot.quantity -= amount;

        if (slot.quantity <= 0)
            slot.Clear();

        OnInventoryChanged?.Invoke();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            DebugInventory();
        }
    }

    private void DebugInventory()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty)
            {
                Debug.Log(
                    "Slot " + i +
                    ": " +
                    slots[i].item.itemName +
                    " x" +
                    slots[i].quantity
                );
            }
        }
    }
}
