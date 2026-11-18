using System.Collections;
using UnityEngine;

public sealed class VillageArrivalDoor : MonoBehaviour
{
    [SerializeField] private Transform hinge;
    [SerializeField] private AudioClip closeSound;
    [SerializeField, Min(0.05f)] private float closeDuration = 0.95f;
    private Quaternion closedRotation;

    private void Awake()
    {
        closedRotation = hinge.localRotation;
        hinge.localRotation = closedRotation * Quaternion.Euler(0f, -95f, 0f);
    }

    private IEnumerator Start()
    {
        while (SceneTransitionFade.InProgress)
            yield return null;

        var openRotation = hinge.localRotation;
        for (float elapsed = 0f; elapsed < closeDuration; elapsed += Time.deltaTime)
        {
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / closeDuration);
            hinge.localRotation = Quaternion.Slerp(openRotation, closedRotation, progress);
            yield return null;
        }

        hinge.localRotation = closedRotation;
        CastleAudioEffects.Play(closeSound, hinge.position, 0.5f);
    }
}
