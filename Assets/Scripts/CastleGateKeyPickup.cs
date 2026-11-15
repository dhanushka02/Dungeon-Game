using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CastleGateKeyPickup : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private TMP_Text pickupPrompt;
    [SerializeField, Min(0.1f)] private float pickupDistance = 2f;

    public bool IsCollected { get; private set; }

    private void Awake()
    {
        SetPrompt(false);
    }

    private void Update()
    {
        bool inRange = player != null && Vector3.Distance(player.position, transform.position) <= pickupDistance;
        SetPrompt(inRange && !IsCollected);
        if (inRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            TryPickUp();
    }

    public bool TryPickUp()
    {
        if (IsCollected || !isActiveAndEnabled || player == null ||
            Vector3.Distance(player.position, transform.position) > pickupDistance)
            return false;

        IsCollected = true;
        SetPrompt(false);
        gameObject.SetActive(false);
        return true;
    }

    private void SetPrompt(bool visible)
    {
        if (pickupPrompt == null) return;
        pickupPrompt.text = "Press E to pick up the castle gate key";
        pickupPrompt.gameObject.SetActive(visible);
    }

    private void OnDisable()
    {
        SetPrompt(false);
    }
}
