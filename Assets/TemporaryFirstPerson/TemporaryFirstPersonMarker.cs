using UnityEngine;

[DisallowMultipleComponent]
public sealed class TemporaryFirstPersonMarker : MonoBehaviour
{
    [SerializeField] private Camera[] sceneCameras;
    [SerializeField] private bool[] cameraEnabledStates;
    [SerializeField] private AudioListener[] sceneAudioListeners;
    [SerializeField] private bool[] audioListenerEnabledStates;

    public void Configure(Camera[] cameras, AudioListener[] listeners)
    {
        sceneCameras = cameras;
        cameraEnabledStates = CaptureStates(cameras);
        sceneAudioListeners = listeners;
        audioListenerEnabledStates = CaptureStates(listeners);
    }

    public void DisableExistingSceneCameras()
    {
        SetEnabled(sceneCameras, false);
        SetEnabled(sceneAudioListeners, false);
    }

    public void RestoreExistingSceneCameras()
    {
        RestoreStates(sceneCameras, cameraEnabledStates);
        RestoreStates(sceneAudioListeners, audioListenerEnabledStates);
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying)
        {
            RestoreExistingSceneCameras();
        }
    }

    private static bool[] CaptureStates(Behaviour[] behaviours)
    {
        var states = new bool[behaviours.Length];
        for (int index = 0; index < behaviours.Length; index++)
        {
            states[index] = behaviours[index] != null && behaviours[index].enabled;
        }
        return states;
    }

    private static void SetEnabled(Behaviour[] behaviours, bool enabled)
    {
        foreach (Behaviour behaviour in behaviours)
        {
            if (behaviour != null)
            {
                behaviour.enabled = enabled;
            }
        }
    }

    private static void RestoreStates(Behaviour[] behaviours, bool[] states)
    {
        int count = Mathf.Min(behaviours.Length, states.Length);
        for (int index = 0; index < count; index++)
        {
            if (behaviours[index] != null)
            {
                behaviours[index].enabled = states[index];
            }
        }
    }
}
