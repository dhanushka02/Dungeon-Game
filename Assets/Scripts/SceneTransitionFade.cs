using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SceneTransitionFade : MonoBehaviour
{
    private const float FadeOutSeconds = 0.55f;
    private const float FadeInSeconds = 0.8f;
    private static SceneTransitionFade activeFade;

    public static bool InProgress => activeFade != null;

    public static bool TryLoad(string sceneName)
    {
        if (InProgress)
            return false;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' is not enabled in the Scene List.");
            return false;
        }

        var overlay = new GameObject("Scene transition fade", typeof(Canvas));
        DontDestroyOnLoad(overlay);
        var canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        var curtain = new GameObject("Black curtain", typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)curtain.transform;
        rect.SetParent(overlay.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        var image = curtain.GetComponent<Image>();
        image.color = Color.clear;
        image.raycastTarget = false;
        activeFade = overlay.AddComponent<SceneTransitionFade>();
        activeFade.StartCoroutine(activeFade.Transition(image, sceneName));
        return true;
    }

    private IEnumerator Transition(Image image, string sceneName)
    {
        yield return Fade(image, 0f, 1f, FadeOutSeconds);
        yield return null; // Render a fully black frame before changing scenes.
        AsyncOperation load = null;
        try
        {
            load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
        catch (System.Exception error)
        {
            Debug.LogException(error);
        }
        if (load != null)
            yield return load;
        yield return null; // Let the destination player and camera initialise under black.
        yield return Fade(image, 1f, 0f, FadeInSeconds);
        Destroy(gameObject);
    }

    private static IEnumerator Fade(Image image, float from, float to, float seconds)
    {
        for (float elapsed = 0f; elapsed < seconds; elapsed += Time.unscaledDeltaTime)
        {
            image.color = new Color(0f, 0f, 0f, Mathf.SmoothStep(from, to, elapsed / seconds));
            yield return null;
        }
        image.color = new Color(0f, 0f, 0f, to);
    }
}
