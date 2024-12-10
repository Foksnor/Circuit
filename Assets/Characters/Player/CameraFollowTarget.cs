using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    [SerializeField] private Vector3 cameraOffset = Vector3.zero;
    private GameObject targetToFollow = null;

    private void Awake()
    {
        MainCamera.CameraFollowTarget = this;
    }

    public void SetCameraFollowTarget(GameObject target)
    {
        targetToFollow = target;
    }

    void Update()
    {
        // Temp x-axis location until I have dynamically changing depth in levels
        if (targetToFollow != null)
            transform.position = targetToFollow.transform.position + cameraOffset;
        else
            Debug.Log("No target to follow main camera with.");
    }
}

public static class MainCamera
{
    public static CameraFollowTarget CameraFollowTarget = null;
}