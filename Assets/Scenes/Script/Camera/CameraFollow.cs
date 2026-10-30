using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    public float distance = 4f;
    public float height = 1.5f;
    public float mouseSpeed = 2f;
    public float smoothSpeed = 10f;

    private float horizontal = 0f;
    private float vertical = 10f;

    void LateUpdate()
    {
        horizontal += Input.GetAxis("Mouse X") * mouseSpeed;
        vertical -= Input.GetAxis("Mouse Y") * mouseSpeed;

        vertical = Mathf.Clamp(vertical, -20f, 60f);

        Quaternion rotation =
            Quaternion.Euler(vertical, horizontal, 0f);

        Vector3 offset =
            rotation * new Vector3(0f, 0f, -distance);

        Vector3 targetPosition =
            player.position + Vector3.up * height + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(
            player.position + Vector3.up * height
        );
    }
}