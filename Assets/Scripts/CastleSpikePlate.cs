using UnityEngine;

public sealed class CastleSpikePlate : MonoBehaviour
{
    [SerializeField] private Transform spikes;
    [SerializeField] private Transform pressureTile;
    [SerializeField] private BoxCollider damageArea;
    [SerializeField] private float phaseOffset;
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioClip movementSound;
    private bool warned, raised;

    public const float CycleDuration = 7.6f;
    public float Height { get; private set; }
    public bool IsWarning { get; private set; }
    public bool IsDangerous => Height > 0.22f;

    public static float HeightAt(float phase)
    {
        phase = Mathf.Repeat(phase, CycleDuration);
        if (phase < 4.4f) return 0f; // 3.6 seconds down, then an audible 0.8-second warning.
        if (phase < 4.8f) return (phase - 4.4f) / 0.4f;
        if (phase < 7.2f) return 1f;
        return 1f - (phase - 7.2f) / 0.4f;
    }

    public void Evaluate(float elapsed, bool audible)
    {
        float phase = Mathf.Repeat(elapsed + phaseOffset, CycleDuration);
        Height = HeightAt(phase);
        IsWarning = phase >= 3.6f && phase < 4.4f;
        spikes.localPosition = Vector3.up * (-1.15f + Height * 1.2f);
        // The perforated deck stays above the stone; fixed socket wells never sink.
        pressureTile.localPosition = Vector3.up * (IsWarning ? -0.008f : 0f);
        damageArea.enabled = IsDangerous;
        damageArea.center = new Vector3(0f, Mathf.Max(0.08f, Height * 1.15f) * 0.5f, 0f);
        Vector3 size = damageArea.size;
        size.y = Mathf.Max(0.08f, Height * 1.15f);
        damageArea.size = size;
        if (audible && IsWarning && !warned)
            CastleAudioEffects.Play(warningSound, transform.position, 0.4f);
        if (audible && Height > 0f && !raised)
            CastleAudioEffects.Play(movementSound, transform.position, 0.3f);
        warned = IsWarning;
        raised = Height > 0f;
    }

    public bool Hits(Bounds body) => IsDangerous && damageArea.bounds.Intersects(body);
}
