using TMPro;
using UnityEngine;

public sealed class QuestCompass : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform target;
    [SerializeField] private RectTransform needle;
    [SerializeField] private TMP_Text distanceText;

    private void LateUpdate()
    {
        if (playerCamera == null || target == null || needle == null) return;

        Vector3 toTarget = target.position - playerCamera.position;
        Vector3 forward = playerCamera.forward;
        toTarget.y = 0f;
        forward.y = 0f;
        if (toTarget.sqrMagnitude < 0.01f || forward.sqrMagnitude < 0.0001f) return;

        float angle = Vector3.SignedAngle(forward, toTarget, Vector3.up);
        needle.localEulerAngles = new Vector3(0f, 0f, -angle);
        if (distanceText != null) distanceText.text = Mathf.RoundToInt(toTarget.magnitude) + "m";
    }
}
