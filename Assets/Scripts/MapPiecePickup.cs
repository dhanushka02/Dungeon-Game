using System.Collections;
using TMPro;
using UnityEngine;

public class MapPiecePickup : InteractablePickup
{
    [Header("Compass")]
    [SerializeField] private QuestCompass compass;
    [SerializeField] private TMP_Text objectiveText;

    [Header("Timing")]
    [SerializeField] private float messageDuration = 5f;

    protected override string InteractionMessage =>
        "Press E to pick up the torn map";

    protected override void Collect()
    {
        CompleteCollection();
        HideMapVisuals();

        if (compass != null)
            compass.gameObject.SetActive(true);

        ShowPrompt(
            "The other half of his map.\n" +
            "The castle on the hill is circled in charcoal."
        );

        if (objectiveText != null)
        {
            objectiveText.text =
                "FIND YOUR SON\n" +
                "Head outside and follow the compass to the castle.";
        }

        StartCoroutine(FinishPickup());
    }

    private void HideMapVisuals()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer mapRenderer in renderers)
            mapRenderer.enabled = false;
    }

    private IEnumerator FinishPickup()
    {
        yield return new WaitForSeconds(messageDuration);

        HidePrompt();
        gameObject.SetActive(false);
    }
}
