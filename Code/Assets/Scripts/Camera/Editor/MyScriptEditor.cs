using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor;


[CustomEditor(typeof(CameraControlTrigger))]

public class MyScriptEditor : Editor
{
    CameraControlTrigger cameraControlTrigger;

    private void OnEnable()
    {
        cameraControlTrigger = (CameraControlTrigger)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (cameraControlTrigger.customInspectorObjects.swapCameras)
        {
            cameraControlTrigger.customInspectorObjects.camera1 = (CinemachineVirtualCamera)EditorGUILayout.ObjectField("Camera 1",
                cameraControlTrigger.customInspectorObjects.camera1, typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;
            cameraControlTrigger.customInspectorObjects.camera2 = (CinemachineVirtualCamera)EditorGUILayout.ObjectField("Camera 2",
                cameraControlTrigger.customInspectorObjects.camera2, typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;

        }

        if (cameraControlTrigger.customInspectorObjects.panCameraOnContact)
        { 
            cameraControlTrigger.customInspectorObjects.panDirection = (PanDirection)EditorGUILayout.EnumPopup("Pan Direction",
                cameraControlTrigger.customInspectorObjects.panDirection);
            cameraControlTrigger.customInspectorObjects.panDistance = EditorGUILayout.FloatField("Pan Distance",
                cameraControlTrigger.customInspectorObjects.panDistance);
            cameraControlTrigger.customInspectorObjects.panTime = EditorGUILayout.FloatField("Pan Time",
                cameraControlTrigger.customInspectorObjects.panTime);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}
