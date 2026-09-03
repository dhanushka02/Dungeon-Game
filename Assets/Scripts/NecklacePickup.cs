using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class NecklacePickup : MonoBehaviour
{
    [Header("Necklace")]
    [SerializeField] private GameObject necklaceVisual;
    [SerializeField] private GameObject magicEffect;

    [Header("UI")]
    [SerializeField] private GameObject promptObject;
    [SerializeField] private TMP_Text promptText;

    [Header("Progression")]
    [SerializeField] private GameObject nextObjective;

    private bool playerInRange;
    private bool collected;

    private void Start()
    {
        if (promptObject != null)
            promptObject.SetActive(false);

        if (nextObjective != null)
            nextObjective.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange &&
            !collected &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            CollectNecklace();
        }
    }

    private void CollectNecklace()
    {
        collected = true;

        if (necklaceVisual != null)
            necklaceVisual.SetActive(false);

        if (magicEffect != null)
            magicEffect.SetActive(false);

        if (nextObjective != null)
            nextObjective.SetActive(true);

        if (promptText != null)
            promptText.text = "Found: Your son's necklace. He was here.";

        if (promptObject != null)
        {
            promptObject.SetActive(true);
            StartCoroutine(HideMessage());
        }
    }

    private IEnumerator HideMessage()
    {
        yield return new WaitForSeconds(4f);

        if (promptObject != null)
            promptObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || collected)
            return;

        playerInRange = true;

        if (promptText != null)
            promptText.text = "Press E to pick up the necklace";

        if (promptObject != null)
            promptObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || collected)
            return;

        playerInRange = false;

        if (promptObject != null)
            promptObject.SetActive(false);
    }
}