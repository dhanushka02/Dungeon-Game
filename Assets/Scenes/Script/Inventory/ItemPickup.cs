
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData item;
    public float pickupRange = 2f;

    private static ItemPickup closestItem;
    private static float closestDistance;

    private Transform player;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void Update()
    {
        if (player == null || InventoryManager.Instance == null)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            ItemPickup[] pickups = FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);

            closestItem = null;
            closestDistance = Mathf.Infinity;

            foreach (ItemPickup pickup in pickups)
            {
                if (pickup.player == null || pickup.item == null)
                    continue;

                float distance = Vector3.Distance(
                    pickup.transform.position,
                    pickup.player.position
                );

                if (distance <= pickup.pickupRange && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = pickup;
                }
            }

            if (closestItem == this)
            {
                bool added = InventoryManager.Instance.AddItem(item);

                if (added)
                    Destroy(gameObject);
            }
        }
    }
}
