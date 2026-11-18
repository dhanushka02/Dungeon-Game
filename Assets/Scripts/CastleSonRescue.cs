using System.Collections;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class CastleSonRescue : MonoBehaviour
{
    [SerializeField] private CharacterController player;
    [SerializeField] private FirstPersonController movement;
    [SerializeField] private AxeInteraction axe;
    [SerializeField] private Transform axeHolder;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CastleGateKeyPickup castleKey;
    [SerializeField] private CastleGuardsRoom guardsRoom;
    [SerializeField] private Transform doorHinge;
    [SerializeField] private CharacterController son;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip doorSound;
    [SerializeField] private Bounds roomBounds = new Bounds(new Vector3(16, 1, 2), new Vector3(12, 4, 8));
    [SerializeField, Min(0.5f)] private float openingDuration = 2.2f;
    [SerializeField] private float openingAngle = -105f;

    [SerializeField, Min(1f)] private float endingFadeDuration = 3f;

    private Quaternion closedDoor;
    private bool entered, ownsControls, previousMovement, previousAxe, previousHolder;
    private bool ending;
    private Vector3 weaponPosition;
    private Quaternion weaponRotation;
    private CursorLockMode previousCursor;
    private bool previousCursorVisible;
    private float messageUntil;
    private string message;
    private string[] dialogue;
    private int line;

    public bool IsOpening { get; private set; }
    public bool IsDoorOpen { get; private set; }
    public bool IsTalking { get; private set; }
    public bool IsComplete { get; private set; }

    private void Awake()
    {
        closedDoor = doorHinge.localRotation;
        promptPanel.SetActive(false);
        dialoguePanel.SetActive(false);
        endingPanel.SetActive(false);
        fade.alpha = 0f;
    }

    private void Update()
    {
        if (ending) return;

        bool use = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool escape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool inside = roomBounds.Contains(transform.InverseTransformPoint(player.transform.position));
        if (!entered && inside)
        {
            entered = true;
            Say("Son: Dad? Over here!", 4f);
        }
        if (IsTalking)
        {
            if (escape) CancelConversation();
            else if (use) AdvanceDialogue();
            return;
        }
        if (!inside) { promptPanel.SetActive(false); return; }
        FacePlayer();

        CastleRescueTarget target = LookTarget();
        if (use && target != null)
        {
            if (target.TargetKind == CastleRescueTarget.Kind.CellDoor) TryUnlock();
            else if (IsDoorOpen && !IsComplete) BeginConversation();
        }
        if (IsTalking) return;
        string hint = target == null ? null : target.TargetKind == CastleRescueTarget.Kind.CellDoor ?
            IsOpening ? "Unlocking the cell..." : IsDoorOpen ? null :
            castleKey != null && castleKey.IsCollected ? "[E] Unlock the cell with the guard's key" :
                "The cell is locked. You need the guard's key from the castle entrance." :
            IsDoorOpen && !IsComplete ? "[E] Talk to your son" : null;
        ShowPrompt(Time.time < messageUntil ? message : hint);
    }

    private CastleRescueTarget LookTarget()
    {
        if (!Physics.Raycast(playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)),
            out var hit, 3.2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) return null;
        var target = hit.collider.GetComponentInParent<CastleRescueTarget>();
        return target != null && target.transform.IsChildOf(transform) ? target : null;
    }

    private void TryUnlock()
    {
        if (IsDoorOpen || IsOpening) return;
        if (castleKey == null || !castleKey.IsCollected)
        { Say("The cell is locked. Find the guard's key near the castle entrance.", 4f); return; }
        IsOpening = true;
        StartCoroutine(OpenCell());
    }

    private IEnumerator OpenCell()
    {
        CastleAudioEffects.Play(unlockSound, doorHinge.position + Vector3.up, 0.5f);
        Say("The guard's key fits the cell lock.", openingDuration + 0.4f);
        yield return new WaitForSeconds(0.3f);
        CastleAudioEffects.Play(doorSound, doorHinge.position + Vector3.up, 0.4f);
        Quaternion open = closedDoor * Quaternion.Euler(0, openingAngle, 0);
        for (float t = 0; t < openingDuration; t += Time.deltaTime)
        {
            doorHinge.localRotation = Quaternion.Slerp(closedDoor, open, Mathf.SmoothStep(0, 1, t / openingDuration));
            yield return null;
        }
        FinishOpening();
        Say("The cell is open. Go to your son and press E to talk.", 4f);
    }

    private void FinishOpening()
    {
        doorHinge.localRotation = closedDoor * Quaternion.Euler(0, openingAngle, 0);
        IsOpening = false;
        IsDoorOpen = true;
    }

    private void BeginConversation()
    {
        TakeControls();
        IsTalking = true;
        dialogue = guardsRoom != null && guardsRoom.KeepsakeCollected ? new[] {
            "Son: I knew you would find me.", "Father: I'm here. You're safe now.",
            "Son: You kept my drawing?", "Father: Of course. We are taking it home.",
            "Father: Come on. We are leaving together." } : new[] {
            "Son: I knew you would find me.", "Father: I'm here. You're safe now.",
            "Father: Come on. We are leaving together." };
        line = 0;
        promptPanel.SetActive(false);
        dialoguePanel.SetActive(true);
        UpdateDialogue();
        StartCoroutine(LowerAxe());
    }

    private void AdvanceDialogue()
    {
        if (++line < dialogue.Length) { UpdateDialogue(); return; }
        IsTalking = false;
        dialoguePanel.SetActive(false);
        StartCoroutine(FinishRescue());
    }

    private void UpdateDialogue() => dialogueText.text = dialogue[line] + "\n\n[E] Continue    [Esc] Close";

    private IEnumerator FinishRescue()
    {
        ending = true;
        promptPanel.SetActive(false);
        if (movement != null) movement.enabled = false;
        float duration = Mathf.Max(1f, endingFadeDuration);
        for (float t = 0; t < duration; t += Time.deltaTime)
        { fade.alpha = Mathf.SmoothStep(0f, 1f, t / duration); yield return null; }
        fade.alpha = 1f;
        IsComplete = true;
        endingPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator LowerAxe()
    {
        if (axeHolder == null) yield break;
        for (float t = 0; t < 0.25f; t += Time.deltaTime)
        { axeHolder.localPosition = Vector3.Lerp(weaponPosition, weaponPosition + Vector3.down * 0.8f, t / 0.25f); yield return null; }
        axeHolder.gameObject.SetActive(false);
    }

    private void TakeControls()
    {
        if (ownsControls) return;
        ownsControls = true;
        previousMovement = movement != null && movement.enabled;
        previousAxe = axe != null && axe.enabled;
        previousHolder = axeHolder != null && axeHolder.gameObject.activeSelf;
        if (axeHolder != null) { weaponPosition = axeHolder.localPosition; weaponRotation = axeHolder.localRotation; }
        previousCursor = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        if (movement != null) movement.enabled = false;
        if (axe != null) axe.enabled = false;
    }

    private void RestoreControls()
    {
        if (!ownsControls) return;
        ownsControls = false;
        if (axeHolder != null) { axeHolder.SetLocalPositionAndRotation(weaponPosition, weaponRotation); axeHolder.gameObject.SetActive(previousHolder); }
        if (movement != null) movement.enabled = previousMovement;
        if (axe != null) axe.enabled = previousAxe;
        Cursor.lockState = previousCursor;
        Cursor.visible = previousCursorVisible;
    }

    private void CancelConversation()
    {
        StopAllCoroutines();
        IsTalking = false;
        dialoguePanel.SetActive(false);
        RestoreControls();
    }

    private void FacePlayer()
    {
        Vector3 toward = player.transform.position - son.transform.position;
        toward.y = 0;
        if (toward.sqrMagnitude > 0.1f)
            son.transform.rotation = Quaternion.Slerp(son.transform.rotation, Quaternion.LookRotation(toward), Time.deltaTime * 3f);
    }

    private void ShowPrompt(string text)
    { promptPanel.SetActive(!string.IsNullOrEmpty(text)); promptText.text = text ?? ""; }
    private void Say(string text, float seconds) { message = text; messageUntil = Time.time + seconds; }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (IsOpening) FinishOpening();
        IsTalking = false;
        ending = false;
        RestoreControls();
        if (promptPanel != null) promptPanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (endingPanel != null) endingPanel.SetActive(false);
        if (fade != null) fade.alpha = 0f;
    }
}
