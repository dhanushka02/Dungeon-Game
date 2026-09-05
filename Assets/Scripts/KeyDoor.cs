using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float interactionDistance = 2.5f;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("UI")]
    [SerializeField] private TMP_Text promptText;

    [Header("Message Settings")]
    [SerializeField] private float messageDuration = 2f;

    private Coroutine messageCoroutine;
    private bool showingNoKeyMessage = false;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= interactionDistance)
        {
            if (promptText != null)
            {
                promptText.gameObject.SetActive(true);

                // Only show the normal prompt if
                // the "need key" message isn't showing
                if (!showingNoKeyMessage)
                {
                    promptText.text = "Press E to open door";
                }
            }

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryOpenDoor();
            }
        }
        else
        {
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }

    private void TryOpenDoor()
    {
        if (KeyPickup.HasKey)
        {
            OpenDoor();
        }
        else
        {
            ShowNoKeyMessage();
        }
    }

    private void ShowNoKeyMessage()
    {
        if (promptText == null)
            return;

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(NoKeyMessage());
    }

    private IEnumerator NoKeyMessage()
    {
        showingNoKeyMessage = true;

        promptText.gameObject.SetActive(true);

        promptText.text = "You need a key to open this door!";

        // Stay on this message for the chosen duration
        yield return new WaitForSeconds(messageDuration);

        showingNoKeyMessage = false;

        // Check if player is still near the door
        if (player != null)
        {
            float distance = Vector3.Distance(
                transform.position,
                player.position
            );

            if (distance <= interactionDistance)
            {
                promptText.text = "Press E to open door";
                promptText.gameObject.SetActive(true);
            }
            else
            {
                promptText.gameObject.SetActive(false);
            }
        }

        messageCoroutine = null;
    }

    private void OpenDoor()
    {
        Debug.Log("Door opened!");

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }

        // Make the door disappear
        gameObject.SetActive(false);
    }
}