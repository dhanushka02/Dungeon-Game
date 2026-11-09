using System.Collections;
using UnityEngine;

public class BreakableBarrel : MonoBehaviour, IAxeBreakable
{
    [SerializeField, Min(1)] private int hitsRequired = 2;
    [SerializeField] private GameObject debris;
    [SerializeField] private GameObject hiddenItem;
    [Header("Audio")]
    [SerializeField] private AudioClip[] impactSounds;
    [SerializeField] private AudioClip breakSound;
    [SerializeField, Range(0f, 1f)] private float impactVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float breakVolume = 0.65f;

    private int hitsTaken;
    private bool broken;
    private Vector3 restingPosition;
    private Quaternion restingRotation;
    private Vector3 restingScale;
    private Renderer barrelRenderer;
    private Coroutine reaction;

    public bool CanBeHit => !broken && isActiveAndEnabled;

    private void Awake()
    {
        restingPosition = transform.localPosition;
        restingRotation = transform.localRotation;
        restingScale = transform.localScale;
        barrelRenderer = GetComponentInChildren<Renderer>();
        if (debris != null) debris.SetActive(false);
        if (hiddenItem != null) hiddenItem.SetActive(false);
    }

    public void Hit()
    {
        Vector3 point = barrelRenderer != null ? barrelRenderer.bounds.center : transform.position;
        Hit(point, Vector3.back, Vector3.forward);
    }

    // World-space contact supplied by the axe; false means no damage was accepted.
    public bool Hit(Vector3 point, Vector3 normal, Vector3 direction)
    {
        if (!CanBeHit || !AxePickup.HasAxe) return false;
        hitsTaken++;
        bool finalHit = hitsTaken >= Mathf.Max(1, hitsRequired);
        CastleAudioEffects.PlayImpact(impactSounds, point, impactVolume);
        float floorY = barrelRenderer != null ? barrelRenderer.bounds.min.y : transform.position.y;
        Material wood = barrelRenderer != null ? barrelRenderer.sharedMaterial : null;
        BarrelImpactEffects.Splinters(point, normal, floorY, wood, finalHit);
        if (!finalHit) BarrelImpactEffects.Cut(transform, point, normal, wood);
        if (reaction != null) StopCoroutine(reaction);
        transform.SetLocalPositionAndRotation(restingPosition, restingRotation);
        transform.localScale = restingScale;
        if (finalHit)
        {
            broken = true;
            CastleAudioEffects.Play(breakSound, point, breakVolume);
            if (debris != null && debris.transform.IsChildOf(transform))
                debris.transform.SetParent(transform.parent, true);
            if (hiddenItem != null && hiddenItem.transform.IsChildOf(transform))
                hiddenItem.transform.SetParent(transform.parent, true);
            foreach (Collider collider in GetComponentsInChildren<Collider>()) collider.enabled = false;
            reaction = StartCoroutine(Break());
        }
        else reaction = StartCoroutine(React(direction));
        return true;
    }

    private IEnumerator React(Vector3 direction)
    {
        Vector3 localDirection = transform.parent != null
            ? transform.parent.InverseTransformDirection(direction) : direction;
        localDirection.y = 0f;
        for (float elapsed = 0f; elapsed < 0.24f; elapsed += Time.deltaTime)
        {
            float t = elapsed / 0.24f;
            float kick = Mathf.Sin(t * Mathf.PI * 3f) * (1f - t);
            transform.localPosition = restingPosition + localDirection.normalized * (kick * 0.035f);
            transform.localRotation = restingRotation * Quaternion.Euler(kick * 4f, 0f, -kick * 2f);
            yield return null;
        }
        transform.SetLocalPositionAndRotation(restingPosition, restingRotation);
        reaction = null;
    }

    private IEnumerator Break()
    {
        for (float elapsed = 0f; elapsed < 0.16f; elapsed += Time.deltaTime)
        {
            float t = Mathf.Clamp01(elapsed / 0.16f);
            transform.localScale = Vector3.Scale(restingScale,
                new Vector3(1f + t * 0.15f, 1f - t * 0.65f, 1f + t * 0.15f));
            yield return null;
        }
        // Preserve the user's placed debris and hidden item as the final state.
        if (debris != null)
        {
            debris.SetActive(true);
        }
        if (hiddenItem != null)
        {
            hiddenItem.SetActive(true);
        }
        gameObject.SetActive(false);
    }
}
