using System.Collections;
using StarterAssets;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public sealed class CastleFloorTraps : MonoBehaviour
{
    [SerializeField] private CharacterController player;
    [SerializeField] private FirstPersonController movement;
    [SerializeField] private AxeInteraction axe;
    [SerializeField] private Transform checkpoint;
    [SerializeField] private CastleRollingDrum roller;
    [SerializeField] private CastleSpikePlate[] plates;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private Bounds roomBounds = new Bounds(new Vector3(0, 1, -2), new Vector3(12, 4, 8));

    private float elapsed, immuneUntil, messageUntil;
    private string message;
    private bool entered, restoreMovement, restoreAxe;
    public bool IsRespawning { get; private set; }
    public int RespawnCount { get; private set; }
    public float Elapsed => elapsed;

    private void Awake()
    {
        fade.alpha = 0f;
        promptPanel.SetActive(false);
        roller.Evaluate(0f, false);
        foreach (var plate in plates) plate.Evaluate(0f, false);
    }

    private void Update()
    {
        bool inside = roomBounds.Contains(transform.InverseTransformPoint(player.transform.position));
        if (!entered && inside)
        {
            entered = true;
            Say("Watch the drum. Wait on clear stone, then cross when the spikes retract.", 7f);
        }
        if (entered) elapsed += Time.deltaTime;
        roller.Evaluate(elapsed, inside && !IsRespawning);
        foreach (var plate in plates) plate.Evaluate(elapsed, inside && !IsRespawning);
        if (IsRespawning) return;
        if (!inside) { promptPanel.SetActive(false); return; }

        if (player.enabled && Time.time >= immuneUntil)
        {
            bool hit = roller.Hits(player.bounds);
            foreach (var plate in plates) hit |= plate.Hits(player.bounds);
            if (hit) { StartCoroutine(Respawn()); return; }
        }

        string text = Time.time < messageUntil ? message : null;
        promptPanel.SetActive(!string.IsNullOrEmpty(text));
        promptText.text = text ?? "";
    }

    private IEnumerator Respawn()
    {
        IsRespawning = true;
        restoreMovement = movement != null && movement.enabled;
        restoreAxe = axe != null && axe.enabled;
        if (movement != null) movement.enabled = false;
        if (axe != null) axe.enabled = false;
        CastleAudioEffects.Play(hitSound, player.transform.position, 0.45f, spatial: false);
        promptPanel.SetActive(true);
        promptText.text = "Caught by a trap... Returning to the entrance.";
        yield return FadeTo(1f, 0.18f);
        yield return new WaitForSeconds(0.12f);
        TeleportToCheckpoint();
        RespawnCount++;
        elapsed = 0f;
        immuneUntil = Time.time + 1.3f;
        yield return FadeTo(0f, 0.28f);
        RestoreControls();
        IsRespawning = false;
        Say("Try again. Your axe, key and collected clues are still with you.", 4f);
    }

    private void TeleportToCheckpoint()
    {
        Vector3 delta = checkpoint.position - player.transform.position;
        bool controllerEnabled = player.enabled;
        player.enabled = false;
        player.transform.SetPositionAndRotation(checkpoint.position, checkpoint.rotation);
        if (movement != null) movement.ResetAfterTeleport();
        Physics.SyncTransforms();
        player.enabled = controllerEnabled;
        CinemachineCore.OnTargetObjectWarped(player.transform, delta);
        if (movement != null && movement.CinemachineCameraTarget != null)
            CinemachineCore.OnTargetObjectWarped(movement.CinemachineCameraTarget.transform, delta);
    }

    private IEnumerator FadeTo(float alpha, float seconds)
    {
        float from = fade.alpha;
        for (float t = 0; t < seconds; t += Time.deltaTime)
        {
            fade.alpha = Mathf.Lerp(from, alpha, t / seconds);
            yield return null;
        }
        fade.alpha = alpha;
    }

    private void RestoreControls()
    {
        if (movement != null) movement.enabled = restoreMovement;
        if (axe != null) axe.enabled = restoreAxe;
    }

    private void Say(string text, float seconds)
    {
        message = text;
        messageUntil = Time.time + seconds;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (IsRespawning)
        {
            TeleportToCheckpoint();
            RestoreControls();
            IsRespawning = false;
            immuneUntil = Time.time + 1.3f;
        }
        if (fade != null) fade.alpha = 0f;
        if (promptPanel != null) promptPanel.SetActive(false);
    }
}
