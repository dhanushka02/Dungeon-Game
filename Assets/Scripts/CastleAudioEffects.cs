using UnityEngine;

// Detached voices let destruction sounds finish after their source object disappears.
public static class CastleAudioEffects
{
    public static void Play(AudioClip clip, Vector3 position, float volume, float pitch = 1f, bool spatial = true)
    {
        if (clip == null || volume <= 0f) return;
        var voice = new GameObject("Castle SFX - " + clip.name);
        voice.transform.position = position;
        var source = voice.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
        source.spatialBlend = spatial ? 1f : 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 2f;
        source.maxDistance = 22f;
        source.dopplerLevel = 0f;
        source.Play();
        Object.Destroy(voice, clip.length / source.pitch + 0.15f);
    }

    public static void PlayImpact(AudioClip[] clips, Vector3 position, float volume)
    {
        if (clips == null || clips.Length == 0) return;
        Play(clips[Random.Range(0, clips.Length)], position, volume, Random.Range(0.95f, 1.05f));
    }
}
