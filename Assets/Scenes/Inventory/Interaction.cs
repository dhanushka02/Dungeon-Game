using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    
    [SerializeField] private ItemData item;
    [SerializeField] private int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        Inventory inventory =
            other.GetComponent<Inventory>();

        if (inventory == null)
            return;

        bool added = inventory.AddItem(item, amount);

        if (added)
        {
            Destroy(gameObject);
        }
    }
}