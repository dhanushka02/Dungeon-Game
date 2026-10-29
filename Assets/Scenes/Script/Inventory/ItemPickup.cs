
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData item;
    public float pickupRange = 2f;

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

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= pickupRange && Input.GetKeyDown(KeyCode.E))
        {
            bool added = InventoryManager.Instance.AddItem(item);

            if (added)
                Destroy(gameObject);
        }
    }
}
