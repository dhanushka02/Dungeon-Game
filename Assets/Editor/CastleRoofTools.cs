#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CastleRoofTools
{
    [MenuItem("Tools/Castle Roof/Toggle Roof Visibility in Scene View")]
    private static void ToggleVisibility()
    {
        var roof = GameObject.Find("Roof");
        if (roof == null) return;
        var visibility = SceneVisibilityManager.instance;
        if (visibility.IsHidden(roof)) visibility.Show(roof, true);
        else visibility.Hide(roof, true);
    }
}
#endif
