using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapPickup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pickupPrompt;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private float messageDuration = 5f;

    [Header("Progression")]
    [SerializeField] private DoorController doorToOpen;

    private TMP_Text pickupPromptText;
    private bool playerInRange;
    private bool mapCollected;

    private void Start()
    {
        if (pickupPrompt != null)
        {
            pickupPromptText =
                pickupPrompt.GetComponentInChildren<TMP_Text>(true);

            pickupPrompt.SetActive(false);
        }

        if (objectiveText != null)
            objectiveText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange ||
            mapCollected ||
            Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
            PickUpMap();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || mapCollected)
            return;

        playerInRange = true;

        if (pickupPromptText != null)
            pickupPromptText.text = "Press E to pick up the map";

        if (pickupPrompt != null)
            pickupPrompt.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || mapCollected)
            return;

        playerInRange = false;

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
    }

    private void PickUpMap()
    {
        mapCollected = true;
        playerInRange = false;

        // Prevent further interaction.
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (Collider itemCollider in colliders)
            itemCollider.enabled = false;

        // Hide the physical map and its visual effects.
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer itemRenderer in renderers)
            itemRenderer.enabled = false;

        // Open the connected door.
        if (doorToOpen != null)
            doorToOpen.OpenDoor();

        // Show information about the collected map.
        if (pickupPromptText != null)
        {
            pickupPromptText.text =
                "Map collected.\n" +
                "It marks nearby chambers and a route deeper into the dungeon.";
        }

        if (pickupPrompt != null)
            pickupPrompt.SetActive(true);

        Debug.Log("Map collected!");

        StartCoroutine(FinishPickup());
    }

    private IEnumerator FinishPickup()
    {
        yield return new WaitForSeconds(messageDuration);

        // Hide the map information message.
        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);

        // Show the next objective after the message disappears.
        if (objectiveText != null)
        {
            objectiveText.text =
                "OBJECTIVE\n" +
                "Search the nearby rooms for clues about your son.";

            objectiveText.gameObject.SetActive(true);
        }

        // Disable the collected map object.
        gameObject.SetActive(false);
    }
}