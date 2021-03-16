using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(EditorCameraRotatorObject))]
public class EditorCameraRotator : Editor
{
    EditorCameraRotatorObject ecro;

    public void OnEnable()
    {
        ecro = (EditorCameraRotatorObject)target;
        SceneView.duringSceneGui += rotateCamera;
    }

    void rotateCamera(SceneView sceneview)
    {
        if (ecro.autoRotate)
        {
            GravityZone gz = GravityZone.getGravityZone(sceneview.camera.transform.position);
            Vector2 up = sceneview.camera.transform.up;
            if (gz)
            {
                if (gz.radialGravity)
                {
                    up = sceneview.camera.transform.position - gz.transform.position;
                }
                else
                {
                    up = gz.transform.up;
                }
            }
            if ((Vector2)sceneview.camera.transform.up != up)
            {
                sceneview.isRotationLocked = false;
                sceneview.camera.transform.up = up;
                sceneview.camera.Render();
                //sceneview.cameraSettings.
                ecro.rotZ = sceneview.camera.transform.eulerAngles.z;
            }
        }
        else
        {
            Quaternion angle = Quaternion.AngleAxis(ecro.rotZ, Vector3.forward);
            if (sceneview.camera.transform.localRotation != angle)
            {
                sceneview.isRotationLocked = false;
                sceneview.camera.transform.localRotation = angle;
                sceneview.camera.Render();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Toggle"))
        {
            ecro.toggle();
        }
    }
}
