
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public ItemData item;
    public int quantity;

    public InventoryItem(ItemData newItem)
    {
        item = newItem;
        quantity = 1;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public int maxSlots = 20;
    public List<InventoryItem> items = new List<InventoryItem>();

    public event Action OnInventoryChanged;

    private void Awake()
    {
        Instance = this;
    }

    public bool AddItem(ItemData item)
    {
        if (item == null)
            return false;

        if (item.canStack)
        {
            foreach (InventoryItem inventoryItem in items)
            {
                if (inventoryItem.item == item)
                {
                    inventoryItem.quantity++;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        if (items.Count >= maxSlots)
            return false;

        items.Add(new InventoryItem(item));
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void RemoveItem(ItemData item)
    {
        foreach (InventoryItem inventoryItem in items)
        {
            if (inventoryItem.item == item)
            {
                inventoryItem.quantity--;

                if (inventoryItem.quantity <= 0)
                    items.Remove(inventoryItem);

                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }
}
