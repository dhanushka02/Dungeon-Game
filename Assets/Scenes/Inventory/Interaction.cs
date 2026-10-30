using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemData item;
    [SerializeField] private int amount = 1;

    private Inventory playerInventory;

    private void OnTriggerEnter(Collider other)
    {
        Inventory inventory = other.GetComponent<Inventory>();

        if (inventory == null)
            return;

        playerInventory = inventory;
    }

    private void OnTriggerExit(Collider other)
    {
        Inventory inventory = other.GetComponent<Inventory>();

        if (inventory == playerInventory)
        {
            playerInventory = null;
        }
    }

    private void Update()
    {
        if (playerInventory == null)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            PickUpItem();
        }
    }

    private void PickUpItem()
    {
        bool added = playerInventory.AddItem(item, amount);

        if (added)
        {
            Destroy(gameObject);
        }
    }
}