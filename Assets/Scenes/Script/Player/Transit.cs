using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Transit : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;
    [Tooltip("Leave empty to load the next scene in the Build Settings list.")]
    [SerializeField] private string destinationScene;

    private bool playerInRange;

    private void Awake()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange || SceneTransitionFade.InProgress || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            LoadNext();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInRange = true;
        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerInRange = false;
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void LoadNext()
    {
        string sceneName = destinationScene;
        if (string.IsNullOrEmpty(sceneName))
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError("Transit: there is no next scene in the Build Settings list.");
                return;
            }
            sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(nextIndex));
        }

        if (SceneTransitionFade.TryLoad(sceneName) && interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private static bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player") ||
               other.transform.root.CompareTag("Player");
    }
}
