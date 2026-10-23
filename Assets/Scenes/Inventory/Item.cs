using UnityEngine;

public enum ItemType
{
    Consumable,
    Weapon,
    Armor,
    Material,
    Quest
}
[CreateAssetMenu(
    fileName = "New Item",
    menuName = "Inventory/Item"
)]
public class ItemData : ScriptableObject
{
    [Header("Basic Information")]
    public string itemID;
    public string itemName;

    [TextArea]
    public string description;

    public Sprite icon;

    [Header("Item Settings")]
    public ItemType itemType;

    public bool stackable = true;

    [Min(1)]
    public int maxStack = 10;
}