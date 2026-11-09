using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Owns only the clue corridor. Earlier axe, barrel and key interactions stay independent.
public sealed class CastleSonPuzzle : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private BreakableWoodenWall entranceWall;
    [SerializeField] private CastlePuzzleTarget toy;
    [SerializeField] private CastlePuzzleTarget[] torches;
    [SerializeField] private Transform gate;
    [SerializeField] private BoxCollider gateBlocker;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private AudioSource puzzleAudio;
    [SerializeField] private AudioSource knocking;
    [SerializeField] private bool enableKnocking = false;
    [SerializeField] private AudioClip lightSound;
    [SerializeField] private AudioClip resetSound;
    [SerializeField] private AudioClip gateSound;
    [SerializeField, Min(0.5f)] private float interactionDistance = 2.4f;
    [SerializeField, Min(0.1f)] private float openingDuration = 2f;
    [SerializeField, Min(2f)] private float gateLift = 3.8f;

    private static readonly CastlePuzzleKind[] Order =
        { CastlePuzzleKind.Moon, CastlePuzzleKind.Sun, CastlePuzzleKind.Star };
    private int progress;
    private bool entered;
    private string message;
    private float messageUntil;
    private Vector3 closedPosition;

    public bool ToyInspected { get; private set; }
    public bool DrawingRead { get; private set; }
    public bool IsSolved { get; private set; }
    public bool IsGateOpen { get; private set; }
    public int LitCount => progress;

    private void Awake()
    {
        closedPosition = gate.localPosition;
        foreach (var torch in torches) torch.SetLit(false);
        knocking.Stop();
        promptPanel.SetActive(false);
        objectivePanel.SetActive(false);
    }

    private void Update()
    {
        if (player == null || playerCamera == null) return;
        if (!entered && entranceWall.IsBroken && Vector3.Distance(player.position, toy.transform.position) < 6f)
            entered = true;
        objectivePanel.SetActive(entered);
        if (!entered) { promptPanel.SetActive(false); return; }

        CastlePuzzleTarget target = LookTarget();
        if (target != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Interact(target);

        string text = Time.time < messageUntil ? message : Hint(target);
        promptPanel.SetActive(!string.IsNullOrEmpty(text));
        promptText.text = text ?? "";
        objectiveText.text = IsGateOpen ? "FIND YOUR SON\nContinue beyond the gate." :
            IsSolved ? "FIND YOUR SON\nThe passage is opening..." :
            DrawingRead ? "THE THREE WATCHERS\nMoon > Sun > Star    |    Lit: " + progress + "/3" :
            ToyInspected ? "FIND YOUR SON\nSearch this passage for the clue he left." :
            "FIND YOUR SON\nSearch the passage for signs of him.";
    }

    private CastlePuzzleTarget LookTarget()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) return null;
        var target = hit.collider.GetComponentInParent<CastlePuzzleTarget>();
        return target != null && target.transform.IsChildOf(transform) ? target : null;
    }

    private string Hint(CastlePuzzleTarget target)
    {
        if (target == null) return null;
        switch (target.Kind)
        {
            case CastlePuzzleKind.Toy: return "[E] Inspect the wooden horse";
            case CastlePuzzleKind.Drawing: return "[E] Read the scratched drawing";
            case CastlePuzzleKind.Gate: return IsSolved ? null : "The gate has no keyhole. Three unlit torches are nearby.";
            default: return IsSolved ? "The watchers are lit. The gate is open." :
                target.IsLit ? target.Kind + " torch is already lit." : "[E] Light the " + target.Kind.ToString().ToLowerInvariant() + " torch";
        }
    }

    private void Interact(CastlePuzzleTarget target)
    {
        if (target.Kind == CastlePuzzleKind.Toy)
        {
            ToyInspected = true;
            Say("This is his wooden horse... He made it through here.\nHe always leaves drawings when he is frightened.", 7f);
        }
        else if (target.Kind == CastlePuzzleKind.Drawing)
        {
            DrawingRead = true;
            Say("His drawing: the moon wakes the sun, then the star shows the way.\nLight the watchers: MOON > SUN > STAR.", 8f);
        }
        else if (target.Kind != CastlePuzzleKind.Gate && !IsSolved && !target.IsLit)
        {
            if (target.Kind != Order[progress])
            {
                progress = 0;
                foreach (var torch in torches) torch.SetLit(false);
                Play(resetSound);
                Say("The flames went out. That was the wrong order.\nLook for a drawing near the barrels.", 4f);
                return;
            }
            target.SetLit(true);
            progress++;
            Play(lightSound);
            Say(target.Kind + " torch lit. (" + progress + "/3)", 1.6f);
            if (progress == Order.Length)
            {
                IsSolved = true;
                Say("The three watchers answered... The gate is opening!", openingDuration);
                StartCoroutine(OpenGate());
            }
        }
    }

    private IEnumerator OpenGate()
    {
        if (gateSound != null)
            CastleAudioEffects.Play(gateSound, gate.position + Vector3.up * 1.5f, 0.7f,
                gateSound.length / Mathf.Max(0.1f, openingDuration));
        Vector3 opened = closedPosition + Vector3.up * gateLift;
        for (float elapsed = 0f; elapsed < openingDuration; elapsed += Time.deltaTime)
        {
            gate.localPosition = Vector3.Lerp(closedPosition, opened, Mathf.SmoothStep(0f, 1f, elapsed / openingDuration));
            yield return null;
        }
        gate.localPosition = opened;
        gateBlocker.enabled = false;
        IsGateOpen = true;
        if (enableKnocking) knocking.Play();
        Say("The path is clear. Keep searching for your son.\nHold on, son. I'm coming.", 7f);
    }

    private void Play(AudioClip clip)
    {
        if (clip != null && puzzleAudio != null) puzzleAudio.PlayOneShot(clip);
    }

    private void Say(string text, float seconds)
    {
        message = text;
        messageUntil = Time.time + seconds;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (IsSolved && !IsGateOpen && gate != null)
        {
            gate.localPosition = closedPosition + Vector3.up * gateLift;
            gateBlocker.enabled = false;
            IsGateOpen = true;
        }
        if (knocking != null) knocking.Stop();
        if (promptPanel != null) promptPanel.SetActive(false);
        if (objectivePanel != null) objectivePanel.SetActive(false);
    }
}
