using System.Collections;
using TMPro;
using UnityEngine;

public class BreakableWoodenWall : MonoBehaviour, IAxeBreakable
{
    [SerializeField] private Transform player;
    [SerializeField] private TMP_Text prompt;
    [SerializeField] private GameObject promptPanel;
    [SerializeField, Min(0.1f)] private float promptDistance = 4f;
    [SerializeField, Min(1)] private int hitsRequired = 3;
    [SerializeField, Min(1f)] private float openedMessageDuration = 5f;
    [Header("Audio")]
    [SerializeField] private AudioClip[] impactSounds;
    [SerializeField] private AudioClip breakSound;
    [SerializeField, Range(0f, 1f)] private float impactVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float breakVolume = 0.7f;

    private Renderer wallRenderer;
    private Collider[] wallColliders;
    private Bounds wallBounds;
    private Vector3 restingPosition;
    private Quaternion restingRotation;
    private int hitsTaken;
    private bool breaking;
    private float openedMessageUntil;
    private Coroutine reaction;

    public bool IsBroken { get; private set; }
    public bool CanBeHit => !breaking && !IsBroken && isActiveAndEnabled;

    private void Awake()
    {
        wallRenderer = GetComponent<Renderer>();
        wallBounds = wallRenderer.bounds;
        wallColliders = GetComponentsInChildren<Collider>();
        restingPosition = transform.localPosition;
        restingRotation = transform.localRotation;
        ShowPrompt(false);
    }

    private void Update()
    {
        if (prompt == null) return;
        if (IsBroken)
        {
            prompt.text = "The wooden wall is broken. A new path has opened!";
            ShowPrompt(Time.time < openedMessageUntil);
            return;
        }
        bool nearby = IsPlayerNear();
        prompt.text = AxePickup.HasAxe
            ? "The path is blocked... but this wall is wooden.\nI can break it with my axe. Left-click to swing."
            : "The path is blocked by a wooden wall.\nI need an axe to break through.";
        ShowPrompt(nearby);
    }

    private bool IsPlayerNear()
    {
        if (player == null || Vector3.Distance(player.position, wallBounds.ClosestPoint(player.position)) > promptDistance)
            return false;
        Vector3 eye = player.position + Vector3.up * 1.5f;
        return Physics.Linecast(eye, wallBounds.center, out RaycastHit hit, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore) && hit.collider.GetComponentInParent<BreakableWoodenWall>() == this;
    }

    public bool Hit(Vector3 point, Vector3 normal, Vector3 direction)
    {
        if (!CanBeHit || !AxePickup.HasAxe) return false;
        hitsTaken++;
        bool finalHit = hitsTaken >= Mathf.Max(1, hitsRequired);
        CastleAudioEffects.PlayImpact(impactSounds, point, impactVolume);
        BarrelImpactEffects.Splinters(point, normal, wallBounds.min.y, wallRenderer.sharedMaterial, finalHit);
        if (reaction != null) StopCoroutine(reaction);
        transform.SetLocalPositionAndRotation(restingPosition, restingRotation);
        if (finalHit)
        {
            breaking = true;
            reaction = StartCoroutine(Break(normal));
        }
        else
        {
            BarrelImpactEffects.Cut(transform, point, normal, wallRenderer.sharedMaterial);
            reaction = StartCoroutine(React());
        }
        return true;
    }

    private IEnumerator React()
    {
        for (float elapsed = 0f; elapsed < 0.22f; elapsed += Time.deltaTime)
        {
            float t = elapsed / 0.22f;
            float kick = Mathf.Sin(t * Mathf.PI * 4f) * (1f - t);
            transform.localRotation = restingRotation * Quaternion.Euler(0f, kick * 0.7f, kick * 0.5f);
            yield return null;
        }
        transform.SetLocalPositionAndRotation(restingPosition, restingRotation);
    }

    private IEnumerator Break(Vector3 normal)
    {
        // Brief final recoil, then the solid mesh gives way to non-blocking wooden pieces.
        yield return React();
        wallRenderer.enabled = false;
        CastleAudioEffects.Play(breakSound, wallBounds.center, breakVolume);
        foreach (Collider collider in wallColliders) collider.enabled = false;
        foreach (Transform child in transform)
            if (child.name == "Axe cut") child.gameObject.SetActive(false);
        BarrelImpactEffects.WallFragments(wallBounds, normal, wallRenderer.sharedMaterial);
        IsBroken = true;
        breaking = false;
        openedMessageUntil = Time.time + openedMessageDuration;
    }

    private void ShowPrompt(bool visible)
    {
        if (promptPanel != null) promptPanel.SetActive(visible);
        else if (prompt != null) prompt.gameObject.SetActive(visible);
    }

    private void OnDisable()
    {
        ShowPrompt(false);
    }
}
