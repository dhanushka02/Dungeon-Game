using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Door1 : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float interactionDistance = 2.5f;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("UI")]
    [SerializeField] private TMP_Text promptText;

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
            promptText.gameObject.SetActive(false);
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
                promptText.text = "Press E to open door";
            }

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                GameOpenDoor();
            }
        }
        else
        {
            if (promptText != null)
                promptText.gameObject.SetActive(false);
        }
    }

    private void GameOpenDoor()
    {
        Debug.Log("Door opened!");

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        gameObject.SetActive(false);

        // Load the next scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }
}
