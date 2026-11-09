
using UnityEngine;

// Legacy item assets remain usable; create new items with Invent.ItemData.
//[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public string description;
    public bool canStack = true;
    public bool canUse;
    public int healAmount = 30;
}
