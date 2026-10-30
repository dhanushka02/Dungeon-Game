using UnityEngine;
using UnityEngine.InputSystem;

public class KeyPickup : MonoBehaviour
{
    public static bool HasKey = false;

    [Header("Pickup Settings")]
    [SerializeField] private float pickupDistance = 2f;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("UI")]
    [SerializeField] private GameObject pickupPrompt;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= pickupDistance)
        {
            if (pickupPrompt != null)
                pickupPrompt.SetActive(true);

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
        HasKey = true;

        Debug.Log("KEY PICKED UP!");
        Debug.Log("Player has key: " + HasKey);

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);

        gameObject.SetActive(false);
    }
}