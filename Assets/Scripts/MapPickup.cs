using UnityEngine;
using UnityEngine.InputSystem;

public class MapPickup : MonoBehaviour
{
    [SerializeField] private GameObject pickupPrompt;

    [SerializeField] private DoorController doorToOpen;

    private bool playerInRange;
    private bool mapCollected;

    private void Start()
    {
        pickupPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange || mapCollected || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            PickUpMap();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !mapCollected)
        {
            playerInRange = true;
            pickupPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            pickupPrompt.SetActive(false);
        }
    }

    private void PickUpMap()
    {
        mapCollected = true;
        pickupPrompt.SetActive(false);

        doorToOpen.OpenDoor();

        Debug.Log("Map collected!");
        gameObject.SetActive(false);
    }
}