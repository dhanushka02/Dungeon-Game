using UnityEngine;
using UnityEngine.InputSystem;

public class AxePickup : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject pickupPrompt;
    [SerializeField] private float pickupDistance = 2.5f;

    public static bool HasAxe { get; private set; }

    private bool collected;

    // Reset inventory whenever a new Play session starts.
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInventory()
    {
        HasAxe = false;
    }

    private void Awake()
    {
        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
    }

    private void Update()
    {
        if (collected || player == null)
            return;

        bool inRange = Vector3.Distance(
            player.position,
            transform.position
        ) <= pickupDistance;

        if (pickupPrompt != null)
            pickupPrompt.SetActive(inRange);

        if (inRange &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            CollectAxe();
        }
    }

    private void CollectAxe()
    {
        collected = true;
        HasAxe = true;

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);

        Debug.Log("Axe collected!");

        gameObject.SetActive(false);
    }
}