using UnityEngine;

public class CameraLock : MonoBehaviour
{
    [Header("Entity Origin")]
    public Transform TargetEntity;

    [Header("Distant Camera")]
    public Vector3 offset = new Vector3(0f,4f,-6f); //Height = 4 - Backsite 6 (meter)
    void LateUpdate()
    {
        if (TargetEntity == null) return;
        transform.position = TargetEntity.position + offset;

        transform.LookAt(TargetEntity);
    }

}