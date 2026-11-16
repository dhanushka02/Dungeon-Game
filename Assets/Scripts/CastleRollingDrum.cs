using UnityEngine;

public sealed class CastleRollingDrum : MonoBehaviour
{
    [SerializeField] private Transform drum;
    [SerializeField] private BoxCollider damageArea;
    [SerializeField] private AudioSource rumble;
    [SerializeField] private Vector3 leftEnd;
    [SerializeField] private Vector3 rightEnd;
    [SerializeField, Min(0.5f)] private float travelSeconds = 2.2f;
    [SerializeField, Min(0.2f)] private float endPause = 0.75f;
    private Bounds sweptArea;
    private bool positioned;

    public bool IsPaused { get; private set; }
    public float Period => 2f * (travelSeconds + endPause);
    public Transform Drum => drum;

    public void Evaluate(float elapsed, bool audible)
    {
        float phase = Mathf.Repeat(elapsed, Period);
        float part;
        if (phase < travelSeconds) part = phase / travelSeconds;
        else if (phase < travelSeconds + endPause) part = 1f;
        else if (phase < 2f * travelSeconds + endPause)
            part = 1f - (phase - travelSeconds - endPause) / travelSeconds;
        else part = 0f;
        IsPaused = phase >= travelSeconds && phase < travelSeconds + endPause ||
            phase >= 2f * travelSeconds + endPause;
        Vector3 previous = transform.position;
        transform.localPosition = Vector3.Lerp(leftEnd, rightEnd, part);
        float distance = Vector3.Distance(leftEnd, rightEnd) * part;
        drum.localRotation = Quaternion.AngleAxis(-distance / 0.52f * Mathf.Rad2Deg, Vector3.forward);
        // Sweeping the volume covers movement between frames, including low-frame-rate play.
        sweptArea = new Bounds(transform.position, damageArea.size);
        if (positioned) sweptArea.Encapsulate(new Bounds(previous, damageArea.size));
        positioned = true;
        if (rumble != null)
        {
            rumble.volume = audible ? IsPaused ? 0.06f : 0.20f : 0f;
            if (audible && !rumble.isPlaying) rumble.Play();
        }
    }

    public bool Hits(Bounds body) => sweptArea.Intersects(body);

    private void OnDisable()
    {
        if (rumble != null) rumble.Stop();
        positioned = false;
    }
}
