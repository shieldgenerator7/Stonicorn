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
            foreach (GravityZone gz in GameObject.FindObjectsOfType<GravityZone>())
            {
                if (gz.mainGravityZone)
                {
                    if (gz.radialGravity)
                    {
                        if (gz.GetComponent<CircleCollider2D>().OverlapPoint(sceneview.camera.transform.position))
                        {
                            if (sceneview.camera.transform.localRotation != gz.gameObject.transform.localRotation)
                            {
                                sceneview.isRotationLocked = false;
                                sceneview.camera.transform.up = sceneview.camera.transform.position - gz.gameObject.transform.position;
                                sceneview.camera.Render();
                                ecro.rotZ = sceneview.camera.transform.eulerAngles.z;
                            }
                            break;
                        }
                    }
                    else
                    {
                        if (gz.GetComponent<PolygonCollider2D>().OverlapPoint(sceneview.camera.transform.position))
                        {
                            if (sceneview.camera.transform.localRotation != gz.gameObject.transform.localRotation)
                            {
                                sceneview.isRotationLocked = false;
                                sceneview.camera.transform.localRotation = gz.gameObject.transform.localRotation;
                                sceneview.camera.Render();
                                ecro.rotZ = gz.transform.eulerAngles.z;
                            }
                            break;
                        }
                    }
                }
            }
        }
        else {
            Quaternion angle = Quaternion.AngleAxis(ecro.rotZ, Vector3.forward);
            if (sceneview.camera.transform.localRotation != angle)
            {
                sceneview.isRotationLocked = false;
                sceneview.camera.transform.localRotation = angle;
                sceneview.camera.Render();
            }
        }
    }
}
