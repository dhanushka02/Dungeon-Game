using UnityEngine;
using UnityEngine.Serialization;

namespace Invent
{
public enum ItemType
{
    Consumable,
    Weapon,
    Armor,
    Material,
    Quest
}

public enum ConsumableType
{
    HP,
    Dmg,
    Shield
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
    public GameObject worldPrefab;


    [Header("Item Settings")]
    public ItemType itemType;

    public bool stackable = true;

    [Header("Usage")]
    public bool canUse = false;
    [Min(0)]
    public int weaponDamage;
    public ConsumableType consumableType;
    [FormerlySerializedAs("healAmount")]
    [Min(0)]
    public int amount;

    [Min(1)]
    public int maxStack = 10;
}
}