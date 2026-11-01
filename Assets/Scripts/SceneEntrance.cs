using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneEntrance : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private string destinationScene = "CastleInterior";

    private bool playerInRange;
    private bool isLoading;

    private void Awake()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange || isLoading || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            EnterCastle();
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

    private void EnterCastle()
    {
        if (!Application.CanStreamedLevelBeLoaded(destinationScene))
        {
            Debug.LogError(
                $"Scene '{destinationScene}' is not enabled in the Scene List."
            );
            return;
        }

        isLoading = true;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        SceneManager.LoadScene(destinationScene);
    }

    private static bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player") ||
               other.transform.root.CompareTag("Player");
    }
}