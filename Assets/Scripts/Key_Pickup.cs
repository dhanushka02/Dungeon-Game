using UnityEngine;
using UnityEngine.InputSystem;

public class KeyPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float pickupDistance = 2f;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("UI")]
    [SerializeField] private GameObject pickupPrompt;

    private void Start()
    {
        // Automatically find the player
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        // Hide prompt at the beginning
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Player is close to the key
        if (distance <= pickupDistance)
        {
            if (pickupPrompt != null)
                pickupPrompt.SetActive(true);

            // New Input System
            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                PickUpKey();
            }
        }
        else
        {
            if (pickupPrompt != null)
                pickupPrompt.SetActive(false);
        }
    }

    private void PickUpKey()
    {
        Debug.Log("KEY PICKED UP!");

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);

        // Remove the key
        gameObject.SetActive(false);
    }
}