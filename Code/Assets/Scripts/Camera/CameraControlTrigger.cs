using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor;
using System.Diagnostics;

public class CameraControlTrigger : MonoBehaviour
{

    public CustomInspectorObjects customInspectorObjects;
    // Start is called before the first frame update

    private Collider2D _coll;

    void Start()
    {
        _coll = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        { 
            if (customInspectorObjects.panCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(customInspectorObjects.panDistance, customInspectorObjects.panTime,
                    customInspectorObjects.panDirection, false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Vector2 exitDirection = (other.transform.position - _coll.bounds.center).normalized;
            UnityEngine.Debug.Log("other.transform.position: " + other.transform.position);
            UnityEngine.Debug.Log("collision bound: " + _coll.bounds.center);
            UnityEngine.Debug.Log("exit direction: " + exitDirection);

            if (customInspectorObjects.swapCameras && customInspectorObjects.camera1 != null && customInspectorObjects.camera2 != null)
            {
                UnityEngine.Debug.Log($"Swapping cameras: {customInspectorObjects.camera1.name} ↔ {customInspectorObjects.camera2.name} with exit direction {exitDirection}");
                CameraManager.instance.SwapCamera(customInspectorObjects.camera1, customInspectorObjects.camera2, exitDirection);
            }

            if (customInspectorObjects.panCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(customInspectorObjects.panDistance, customInspectorObjects.panTime,
                    customInspectorObjects.panDirection, true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

[System.Serializable]

public class CustomInspectorObjects
{
    public bool swapCameras = false;
    public bool panCameraOnContact = false;

    [HideInInspector] public CinemachineVirtualCamera camera1;
    [HideInInspector] public CinemachineVirtualCamera camera2;

    [HideInInspector] public PanDirection panDirection;
    [HideInInspector] public float panDistance = 3.0f;
    [HideInInspector] public float panTime = 0.35f;
}

public enum PanDirection
{
    Up,
    Down,
    Left,
    Right
}


