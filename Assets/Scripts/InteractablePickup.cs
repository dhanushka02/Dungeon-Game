using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InteractablePickup : MonoBehaviour
{
    [Header("Shared Interaction")]
    [SerializeField] protected GameObject interactionPrompt;

    protected TMP_Text interactionPromptText;
    protected bool playerInRange;
    protected bool collected;

    protected abstract string InteractionMessage { get; }

    protected virtual void Awake()
    {
        if (interactionPrompt != null)
        {
            interactionPromptText =
                interactionPrompt.GetComponentInChildren<TMP_Text>(true);
        }
    }

    protected virtual void Start()
    {
        HidePrompt();
    }

    protected virtual void Update()
    {
        if (!playerInRange ||
            collected ||
            Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
            Collect();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other) || collected)
            return;

        playerInRange = true;
        ShowPrompt(InteractionMessage);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other) || collected)
            return;

        playerInRange = false;
        HidePrompt();
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player") ||
               other.transform.root.CompareTag("Player");
    }

    protected void ShowPrompt(string message)
    {
        if (interactionPromptText != null)
            interactionPromptText.text = message;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    protected void HidePrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    protected void CompleteCollection()
    {
        collected = true;
        playerInRange = false;

        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (Collider itemCollider in colliders)
            itemCollider.enabled = false;

        HidePrompt();
    }

    protected abstract void Collect();
}






