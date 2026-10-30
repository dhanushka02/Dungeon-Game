using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Test2;

public class Cam : MonoBehaviour
{
    public Transform target;
    public Transform camera;
    public Transform pivot;
    private Transform Ptransform;
    private LayerMask ignoreLayers;

    public static Cam sigleton;

    public float lookSpeed = 0.5f;
    public float followSpeed = 0.1f;
    public float pivotSpeed = 0.3f;


    private float defaultPosition;
    private float LookAngle;
    private float pivotAngle;
    public float minimumPivot = -35;
    public float maximumPivot = 35;



    private void Awake()
    {
        sigleton = this;
        Ptransform = transform;
        defaultPosition =  camera.localPosition.z;
        ignoreLayers = ~(1<<8 | 1<< 9 | 1<<10);
    }

    private void LateUpdate()
    {
        float delta = Time.deltaTime;
        FollowTarget(delta);
    }
    public void FollowTarget(float delta)
    {
        Vector3 targetPosition = Vector3.Lerp(Ptransform.position, target.position, delta/followSpeed);
        Ptransform.position = targetPosition;
    }

    public void HanldeCameraRotation(float delta, float MouseXI, float MouseYI)
    {
        LookAngle += (MouseXI * lookSpeed) / delta;
        pivotAngle -= (MouseYI * lookSpeed)/ delta;
        pivotAngle =  Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot);

        Vector3 rotation =  Vector3.zero;
        rotation.y = LookAngle;
        Quaternion targetRotation = Quaternion.Euler(rotation);
        Ptransform.rotation = targetRotation;

        rotation = Vector3.zero;
        rotation.x = pivotAngle;

        targetRotation = Quaternion.Euler(rotation);
        pivot.localRotation = targetRotation;
    }
}