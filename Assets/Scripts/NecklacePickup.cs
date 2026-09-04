using System.Collections;
using UnityEngine;

public class NecklacePickup : InteractablePickup
{
    [Header("Necklace")]
    [SerializeField] private GameObject necklaceVisual;
    [SerializeField] private GameObject magicEffect;
    [SerializeField] private GameObject nextObjective;
    [SerializeField] private float clueMessageDuration = 4f;

    protected override string InteractionMessage =>
        "Press E to pick up the necklace";

    protected override void Start()
    {
        base.Start();

        if (nextObjective != null)
            nextObjective.SetActive(false);
    }

    protected override void Collect()
    {
        CompleteCollection();

        if (necklaceVisual != null)
            necklaceVisual.SetActive(false);

        if (magicEffect != null)
            magicEffect.SetActive(false);

        if (nextObjective != null)
            nextObjective.SetActive(true);

        ShowPrompt(
            "Found: Your son's necklace.\n" +
            "He was here."
        );

        Debug.Log("Necklace collected!");

        StartCoroutine(HideClueMessage());
    }

    private IEnumerator HideClueMessage()
    {
        yield return new WaitForSeconds(clueMessageDuration);

        HidePrompt();
    }
}