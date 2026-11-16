using UnityEngine;
using UnityEngine.InputSystem;

public class SceneEntrance : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private string destinationScene = "CastleInterior";
    [SerializeField] private bool loadOnEnter;

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
            EnterDestination();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        if (loadOnEnter)
        {
            EnterDestination();
            return;
        }

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

    private void EnterDestination()
    {
        if (SceneTransitionFade.TryLoad(destinationScene) && interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private static bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player") ||
               other.transform.root.CompareTag("Player");
    }
}
