
using UnityEngine;

public class PlayerPotionUse : MonoBehaviour
{
    public ItemData healthPotion;
    public PlayerHealth playerHealth;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            UsePotion();
        }
    }

    void UsePotion()
    {
        if (InventoryManager.Instance == null)
            return;

        if (healthPotion == null || playerHealth == null)
            return;

        if (!playerHealth.CanHeal())
            return;

        foreach (InventoryItem inventoryItem in InventoryManager.Instance.items)
        {
            if (inventoryItem.item == healthPotion && inventoryItem.quantity > 0)
            {
                playerHealth.Heal(healthPotion.healAmount);
                InventoryManager.Instance.RemoveItem(healthPotion);
                return;
            }
        }
    }
}
