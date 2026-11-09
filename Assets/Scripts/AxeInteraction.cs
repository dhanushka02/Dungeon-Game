using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AxeInteraction : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform axeHolder;
    [SerializeField] private float hitDistance = 3f;
    [SerializeField] private float hitCooldown = 0.5f;
    [Header("Pickup hint")]
    [SerializeField] private GameObject pickupHint;
    [SerializeField, Min(1f)] private float pickupHintDuration = 8f;
    [Header("Swing")]
    [SerializeField] private Vector3 swingAngles = new Vector3(65f, -15f, -25f);
    [SerializeField] private Vector3 windupAngles = new Vector3(-30f, 12f, 15f);
    [SerializeField] private Transform axeModel;
    [Tooltip("A point on the cutting edge, in the held axe mesh's local space.")]
    [SerializeField] private Vector3 bladeContactPoint = new Vector3(0f, 0.5031f, -0.1689f);
    [Header("Audio")]
    [SerializeField] private AudioClip swingSound;
    [SerializeField, Range(0f, 1f)] private float swingVolume = 0.3f;

    private Vector3 restingPosition;
    private Quaternion restingRotation;
    private Vector3 bladeOffset;
    private float nextHitTime;
    private float hitMarkerUntil;
    private bool swinging;
    private bool poseInitialized;
    private bool aimingAtBreakable;
    private bool previouslyEquipped;
    private float pickupHintUntil;

    private void Awake()
    {
        if (pickupHint != null) pickupHint.SetActive(false);
        if (axeHolder == null) return;
        restingPosition = axeHolder.localPosition;
        restingRotation = axeHolder.localRotation;
        if (axeModel == null && axeHolder.childCount > 0)
            axeModel = axeHolder.GetChild(0);
        if (axeModel != null)
            bladeOffset = axeHolder.InverseTransformPoint(axeModel.TransformPoint(bladeContactPoint));
        poseInitialized = true;
        axeHolder.gameObject.SetActive(false);
    }

    private void Update()
    {
        bool equipped = AxePickup.HasAxe;
        if (equipped && !previouslyEquipped)
            pickupHintUntil = Time.time + pickupHintDuration;
        previouslyEquipped = equipped;
        if (pickupHint != null)
            pickupHint.SetActive(equipped && Time.time < pickupHintUntil);
        if (axeHolder != null)
            axeHolder.gameObject.SetActive(AxePickup.HasAxe);
        aimingAtBreakable = AxePickup.HasAxe && TryGetTarget(out _, out _);
        if (!AxePickup.HasAxe || axeHolder == null || playerCamera == null || Mouse.current == null || swinging)
            return;
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextHitTime)
        {
            nextHitTime = Time.time + Mathf.Max(hitCooldown, 0.64f);
            StartCoroutine(Swing());
        }
    }

    private IEnumerator Swing()
    {
        swinging = true;
        Vector3 windupPosition = restingPosition + new Vector3(0.08f, 0.06f, -0.12f);
        Quaternion windupRotation = restingRotation * Quaternion.Euler(windupAngles);
        yield return MoveAxe(windupPosition, windupRotation, 0.14f, false);

        CastleAudioEffects.Play(swingSound, playerCamera.transform.position, swingVolume,
            Random.Range(0.97f, 1.03f), false);

        Quaternion strikeRotation = restingRotation * Quaternion.Euler(swingAngles);
        Vector3 strikePosition = restingPosition + new Vector3(-0.18f, -0.08f, 0.15f);
        bool hasTarget = TryGetTarget(out RaycastHit targetHit, out IAxeBreakable target);
        if (hasTarget && axeModel != null)
            strikePosition = ContactPosition(targetHit, strikeRotation);
        yield return MoveAxe(strikePosition, strikeRotation, 0.16f, true);

        // Recheck at contact so looking away or an intervening wall cancels the hit.
        if (hasTarget && TryGetTarget(out RaycastHit hit, out IAxeBreakable targetAtContact) && targetAtContact == target)
        {
            if (axeModel != null)
                axeHolder.localPosition = ContactPosition(hit, strikeRotation);
            if (targetAtContact.Hit(hit.point, hit.normal, playerCamera.transform.forward))
                hitMarkerUntil = Time.time + 0.16f;
        }
        yield return new WaitForSeconds(0.055f);
        yield return MoveAxe(restingPosition, restingRotation, 0.24f, false);
        swinging = false;
    }

    private Vector3 ContactPosition(RaycastHit hit, Quaternion rotation)
    {
        Vector3 point = axeHolder.parent.InverseTransformPoint(hit.point + hit.normal * 0.006f);
        return point - rotation * Vector3.Scale(bladeOffset, axeHolder.localScale);
    }

    private IEnumerator MoveAxe(Vector3 position, Quaternion rotation, float duration, bool accelerate)
    {
        Vector3 fromPosition = axeHolder.localPosition;
        Quaternion fromRotation = axeHolder.localRotation;
        for (float elapsed = 0f; elapsed < duration;)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = accelerate ? t * t : Mathf.SmoothStep(0f, 1f, t);
            axeHolder.localPosition = Vector3.Lerp(fromPosition, position, t);
            axeHolder.localRotation = Quaternion.Slerp(fromRotation, rotation, t);
            yield return null;
        }
        axeHolder.SetLocalPositionAndRotation(position, rotation);
    }

    private bool TryGetTarget(out RaycastHit hit, out IAxeBreakable target)
    {
        target = null;
        hit = default;
        if (playerCamera == null) return false;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out hit, hitDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            target = hit.collider.GetComponentInParent<IAxeBreakable>();
        return target != null && target.CanBeHit;
    }

    private void OnGUI()
    {
        if (!AxePickup.HasAxe || playerCamera == null) return;
        Rect viewport = playerCamera.pixelRect;
        float x = viewport.center.x;
        float y = Screen.height - viewport.center.y;
        Color previous = GUI.color;
        GUI.color = aimingAtBreakable ? new Color(1f, 0.8f, 0.35f) : new Color(1f, 1f, 1f, 0.7f);
        GUI.DrawTexture(new Rect(x - 2f, y - 2f, 4f, 4f), Texture2D.whiteTexture);
        if (Time.time < hitMarkerUntil)
        {
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(x - 11f, y - 1f, 6f, 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x + 5f, y - 1f, 6f, 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x - 1f, y - 11f, 2f, 6f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x - 1f, y + 5f, 2f, 6f), Texture2D.whiteTexture);
        }
        GUI.color = previous;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        swinging = false;
        if (pickupHint != null) pickupHint.SetActive(false);
        if (axeHolder != null && poseInitialized)
            axeHolder.SetLocalPositionAndRotation(restingPosition, restingRotation);
    }
}
