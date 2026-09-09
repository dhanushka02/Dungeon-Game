using System.Collections;
using TMPro;
using UnityEngine;

public class MapPickup : InteractablePickup
{
    [Header("Map Progression")]
    [SerializeField] private DoorController doorToOpen;
    [SerializeField] private GameObject rockBlockage;
    [SerializeField] private TMP_Text objectiveText;

    [Header("Timing")]
    [SerializeField] private float rockfallDelay = 1.5f;
    [SerializeField] private float messageDuration = 5f;

    protected override string InteractionMessage =>
        "Press E to pick up the map";

    protected override void Start()
    {
        base.Start();

        if (objectiveText != null)
            objectiveText.gameObject.SetActive(false);

        if (rockBlockage != null)
            rockBlockage.SetActive(false);
    }

    protected override void Collect()
    {
        CompleteCollection();
        HideMapVisuals();

        if (doorToOpen != null)
            doorToOpen.OpenDoor();

        ShowPrompt(
            "Map collected.\n" +
            "It marks nearby chambers and a route deeper into the dungeon."
        );

        StartCoroutine(ActivateRockBlockage());
        StartCoroutine(FinishPickup());

        Debug.Log("Map collected!");
    }

    private void HideMapVisuals()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer mapRenderer in renderers)
            mapRenderer.enabled = false;

        Light[] lights = GetComponentsInChildren<Light>();

        foreach (Light mapLight in lights)
            mapLight.enabled = false;
    }

    private IEnumerator ActivateRockBlockage()
    {
        yield return new WaitForSeconds(rockfallDelay);

        if (rockBlockage != null)
            rockBlockage.SetActive(true);
    }

    private IEnumerator FinishPickup()
    {
        yield return new WaitForSeconds(messageDuration);

        HidePrompt();

        if (objectiveText != null)
        {
            objectiveText.text =
                "OBJECTIVE\n" +
                "The way back has collapsed.\n" +
                "Search the newly opened rooms for clues about your son.";

            objectiveText.gameObject.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}