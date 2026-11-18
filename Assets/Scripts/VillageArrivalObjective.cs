using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public sealed class VillageArrivalObjective : MonoBehaviour
{
    private IEnumerator Start()
    {
        var label = GetComponent<TextMeshProUGUI>();
        label.enabled = false;
        while (SceneTransitionFade.InProgress)
            yield return null;

        label.enabled = true;
        yield return new WaitForSecondsRealtime(6f);
        gameObject.SetActive(false);
    }
}
