using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openingDuration = 1.5f;

    private Quaternion closedRotation;
    private bool isOpen;

    private void Awake()
    {
        closedRotation = transform.localRotation;
    }

    public void OpenDoor()
    {
        if (isOpen)
            return;

        isOpen = true;
        StartCoroutine(OpenDoorRoutine());
    }

    private IEnumerator OpenDoorRoutine()
    {
        Quaternion startingRotation = transform.localRotation;

        Quaternion targetRotation =
            closedRotation * Quaternion.Euler(0f, openAngle, 0f);

        float elapsedTime = 0f;

        while (elapsedTime < openingDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / openingDuration
            );

            progress = Mathf.SmoothStep(0f, 1f, progress);

            transform.localRotation = Quaternion.Slerp(
                startingRotation,
                targetRotation,
                progress
            );

            yield return null;
        }

        transform.localRotation = targetRotation;
    }
}