using System.Linq;
using StarterAssets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TemporaryFirstPersonInstaller
{
    private const string RootName = "TEMP_FirstPerson_Setup";
    private const string PlayerPrefabPath =
        "Assets/StarterAssets/FirstPersonController/Prefabs/PlayerCapsule.prefab";
    private const string CameraPrefabPath =
        "Assets/StarterAssets/FirstPersonController/Prefabs/MainCamera.prefab";
    private const string FollowCameraPrefabPath =
        "Assets/StarterAssets/FirstPersonController/Prefabs/PlayerFollowCamera.prefab";

    [MenuItem("Tools/Temporary First Person/Add To Open Scene")]
    public static void AddToOpenScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("Open and select a scene before adding the temporary player.");
            return;
        }

        GameObject existing = FindRoot(scene);
        if (existing != null)
        {
            Selection.activeGameObject = existing;
            EditorGUIUtility.PingObject(existing);
            Debug.Log($"{RootName} already exists in {scene.name}.");
            return;
        }

        GameObject playerPrefab = LoadPrefab(PlayerPrefabPath);
        GameObject cameraPrefab = LoadPrefab(CameraPrefabPath);
        GameObject followCameraPrefab = LoadPrefab(FollowCameraPrefabPath);
        if (playerPrefab == null || cameraPrefab == null || followCameraPrefab == null)
        {
            return;
        }

        var root = new GameObject(RootName);
        SceneManager.MoveGameObjectToScene(root, scene);
        root.transform.position = FindSpawnPosition(scene);
        Undo.RegisterCreatedObjectUndo(root, "Add temporary first-person player");

        Camera[] existingCameras = scene.GetRootGameObjects()
            .SelectMany(item => item.GetComponentsInChildren<Camera>(true))
            .ToArray();
        AudioListener[] existingListeners = scene.GetRootGameObjects()
            .SelectMany(item => item.GetComponentsInChildren<AudioListener>(true))
            .ToArray();

        TemporaryFirstPersonMarker marker = root.AddComponent<TemporaryFirstPersonMarker>();
        marker.Configure(existingCameras, existingListeners);

        GameObject player = InstantiatePrefab(playerPrefab, scene, root.transform);
        GameObject mainCamera = InstantiatePrefab(cameraPrefab, scene, root.transform);
        GameObject followCamera = InstantiatePrefab(followCameraPrefab, scene, root.transform);
        player.name = "TEMP_PlayerCapsule";
        mainCamera.name = "TEMP_MainCamera";
        followCamera.name = "TEMP_PlayerFollowCamera";

        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;
        Transform cameraTarget = player.transform.Find("PlayerCameraRoot");
        FirstPersonController controller = player.GetComponent<FirstPersonController>();
        MonoBehaviour virtualCamera = followCamera.GetComponents<MonoBehaviour>()
            .FirstOrDefault(component =>
            {
                SerializedObject serializedComponent = new SerializedObject(component);
                return serializedComponent.FindProperty("m_Follow") != null
                    && serializedComponent.FindProperty("m_LookAt") != null;
            });
        if (cameraTarget == null || controller == null || virtualCamera == null)
        {
            Object.DestroyImmediate(root);
            Debug.LogError("Starter Assets prefabs are missing required first-person components.");
            return;
        }

        controller.CinemachineCameraTarget = cameraTarget.gameObject;
        SerializedObject serializedCamera = new SerializedObject(virtualCamera);
        SerializedProperty followProperty = serializedCamera.FindProperty("m_Follow");
        SerializedProperty lookAtProperty = serializedCamera.FindProperty("m_LookAt");
        followProperty.objectReferenceValue = cameraTarget;
        lookAtProperty.objectReferenceValue = cameraTarget;
        serializedCamera.ApplyModifiedPropertiesWithoutUndo();
        marker.DisableExistingSceneCameras();

        EditorUtility.SetDirty(marker);
        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(virtualCamera);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = root;
        SceneView.lastActiveSceneView?.FrameSelected();

        Debug.Log($"Added {RootName} to {scene.name} at {root.transform.position}. " +
                  "Remove it with Tools > Temporary First Person > Remove From Open Scene.");
    }

    [MenuItem("Tools/Temporary First Person/Remove From Open Scene")]
    public static void RemoveFromOpenScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject root = FindRoot(scene);
        if (root == null)
        {
            Debug.Log($"No {RootName} was found in {scene.name}.");
            return;
        }

        TemporaryFirstPersonMarker marker = root.GetComponent<TemporaryFirstPersonMarker>();
        marker?.RestoreExistingSceneCameras();
        Undo.DestroyObjectImmediate(root);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Removed {RootName} from {scene.name} and restored its previous cameras.");
    }

    [MenuItem("Tools/Temporary First Person/Add To Open Scene", true)]
    private static bool CanAdd()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    [MenuItem("Tools/Temporary First Person/Remove From Open Scene", true)]
    private static bool CanRemove()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static GameObject LoadPrefab(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"Required Starter Assets prefab was not found: {path}");
        }
        return prefab;
    }

    private static GameObject InstantiatePrefab(GameObject prefab, Scene scene, Transform parent)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(instance, "Add temporary first-person player");
        return instance;
    }

    private static GameObject FindRoot(Scene scene)
    {
        return scene.IsValid()
            ? scene.GetRootGameObjects().FirstOrDefault(item => item.name == RootName)
            : null;
    }

    private static Vector3 FindSpawnPosition(Scene scene)
    {
        Vector3 pivot = SceneView.lastActiveSceneView != null
            ? SceneView.lastActiveSceneView.pivot
            : Vector3.zero;

        Physics.SyncTransforms();
        Vector3 rayOrigin = pivot + Vector3.up * 500f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1000f, ~0,
                QueryTriggerInteraction.Ignore))
        {
            return hit.point + Vector3.up * 1.1f;
        }

        Collider fallback = scene.GetRootGameObjects()
            .SelectMany(item => item.GetComponentsInChildren<Collider>(true))
            .Where(collider => collider.enabled && !collider.isTrigger)
            .OrderBy(collider => Vector3.SqrMagnitude(collider.bounds.center - pivot))
            .FirstOrDefault();
        return fallback != null
            ? new Vector3(fallback.bounds.center.x, fallback.bounds.max.y + 1.1f, fallback.bounds.center.z)
            : pivot + Vector3.up * 1.1f;
    }
}
