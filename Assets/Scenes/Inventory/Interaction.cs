using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Invent
{
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemData item;
        [SerializeField] private int amount = 1;

        private Inventory playerInventory;
        private bool pickupPending;

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
            if (pickupPending || playerInventory == null)
                return;

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                PickUpItem();
            }
        }

        private void PickUpItem()
        {
            Test2.Animation playerAnimation =
                playerInventory.GetComponent<Test2.Animation>();
            if (playerAnimation != null && !playerAnimation.CanStartAction)
                return;

            bool added = playerInventory.AddItem(item, amount);

            if (added)
            {
                pickupPending = true;

                if (playerAnimation != null && playerAnimation.PlayPickup())
                {
                    StartCoroutine(DestroyAfterPickupAnimation(playerAnimation));
                    return;
                }

                Destroy(gameObject);
            }
        }

        private IEnumerator DestroyAfterPickupAnimation(Test2.Animation playerAnimation)
        {
            yield return playerAnimation.WaitForPickupAnimation();
            Destroy(gameObject);
        }
    }
}