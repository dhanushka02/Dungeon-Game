using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class HouseDoor : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject prompt;
    [SerializeField] private float interactionDistance = 2.2f;
    [SerializeField] private InteractablePickup requiredItem;
    [SerializeField] private Transform doorLeaf;
    [SerializeField] private float openingAngle = -100f;
    [SerializeField, Min(0.1f)] private float openingDuration = 1.5f;
    [Header("Audio")]
    [SerializeField] private AudioClip openingSound;
    [SerializeField, Range(0f, 1f)] private float openingVolume = 0.65f;

    private TMP_Text promptText;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    public bool IsOpen { get; private set; }
    public bool IsOpening { get; private set; }

    private void Awake()
    {
        if (prompt != null) promptText = prompt.GetComponentInChildren<TMP_Text>(true);
        closedRotation = doorLeaf.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openingAngle, 0f);
        SetPrompt(false);
    }

    private void Update()
    {
        bool nearDoor = IsPlayerNear();
        SetPrompt(nearDoor && !IsOpen);
        if (nearDoor && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            TryOpen();
    }

    private bool IsPlayerNear()
    {
        return player != null && Vector3.Distance(player.position, transform.position) <= interactionDistance;
    }

    public bool TryOpen()
    {
        if (!isActiveAndEnabled || IsOpen || IsOpening || !IsPlayerNear() ||
            (requiredItem != null && !requiredItem.IsCollected))
            return false;

        IsOpening = true;
        StartCoroutine(OpenDoor());
        return true;
    }

    private IEnumerator OpenDoor()
    {
        if (openingSound != null)
            CastleAudioEffects.Play(openingSound, doorLeaf.position, openingVolume, openingSound.length / openingDuration);
        for (float elapsed = 0f; elapsed < openingDuration; elapsed += Time.deltaTime)
        {
            doorLeaf.localRotation = Quaternion.Slerp(closedRotation, openRotation, Mathf.SmoothStep(0f, 1f, elapsed / openingDuration));
            yield return null;
        }
        FinishOpening();
    }

    private void FinishOpening()
    {
        doorLeaf.localRotation = openRotation;
        IsOpening = false;
        IsOpen = true;
        SetPrompt(false);
    }

    private void SetPrompt(bool visible)
    {
        if (prompt == null) return;
        if (promptText != null)
        {
            promptText.text = IsOpening ? "Opening the door..." :
                requiredItem == null || requiredItem.IsCollected ? "Press E to open the door" :
                "Not yet. He must have left something here for me.";
        }
        prompt.SetActive(visible);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (IsOpening && doorLeaf != null) FinishOpening();
        SetPrompt(false);
    }
}
