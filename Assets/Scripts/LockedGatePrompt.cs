using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LockedGatePrompt : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject prompt;
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private CastleGateKeyPickup castleKey;
    [SerializeField] private Transform gateLeaf;
    [SerializeField] private float openingAngle = 100f;
    [SerializeField, Min(0.1f)] private float openingDuration = 1.5f;
    [Header("Audio")]
    [SerializeField] private AudioClip openingSound;
    [SerializeField, Range(0f, 1f)] private float openingVolume = 0.65f;

    private TMP_Text promptText;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool initialized;

    public bool IsOpen { get; private set; }
    public bool IsOpening { get; private set; }

    private void Awake()
    {
        if (prompt != null) promptText = prompt.GetComponent<TMP_Text>();
        if (gateLeaf != null)
        {
            closedRotation = gateLeaf.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, openingAngle, 0f);
            initialized = true;
        }
        SetPrompt(false);
    }

    private void Update()
    {
        bool nearGate = IsPlayerNear();
        SetPrompt(nearGate && !IsOpen);
        if (nearGate && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            TryOpen();
    }

    private bool IsPlayerNear()
    {
        return player != null && Vector3.Distance(player.position, transform.position) <= interactionDistance;
    }

    public bool TryOpen()
    {
        if (!isActiveAndEnabled || !initialized || IsOpen || IsOpening || !IsPlayerNear() ||
            castleKey == null || !castleKey.IsCollected)
            return false;

        IsOpening = true;
        StartCoroutine(OpenGate());
        return true;
    }

    private IEnumerator OpenGate()
    {
        float duration = Mathf.Max(0.1f, openingDuration);
        if (openingSound != null)
            CastleAudioEffects.Play(openingSound, gateLeaf.position, openingVolume, openingSound.length / duration);
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            gateLeaf.localRotation = Quaternion.Slerp(closedRotation, openRotation, t);
            yield return null;
        }
        FinishOpening();
    }

    private void FinishOpening()
    {
        gateLeaf.localRotation = openRotation;
        IsOpening = false;
        IsOpen = true;
        SetPrompt(false);
    }

    private void SetPrompt(bool visible)
    {
        if (prompt == null) return;
        if (promptText != null)
        {
            promptText.text = IsOpening ? "Opening gate..." :
                castleKey != null && castleKey.IsCollected ? "Press E to unlock the gate" : "The gate is locked. Explore the castle and find the key.";
        }
        prompt.SetActive(visible);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (initialized && IsOpening && gateLeaf != null) FinishOpening();
        SetPrompt(false);
    }
}
