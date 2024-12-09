using System.Reflection;
using UnityEditor;
using UnityEngine;
//using Utils;

// ReSharper disable CheckNamespace

//2024-12-08: copied from https://discussions.unity.com/t/how-to-rotate-editor-camera-in-2d/183893/3

namespace EditorTools
{
    public class EditorCameraRotator : EditorWindow
    {
        private float _z = 0;


        [MenuItem("Tools/Editor Camera Rotator")]
        public static void OpenWindow()
        {
            GetWindow<EditorCameraRotator>();
        }

        public void OnEnable()
        {
            SceneView.duringSceneGui -= RotateCamera;
            SceneView.duringSceneGui += RotateCamera;
        }

        private void OnGUI()
        {
            float prevZ = _z;
            EditorGUILayout.BeginHorizontal();
            _z = EditorGUILayout.FloatField(_z, GUILayout.Width(30));
            _z = EditorGUILayout.Slider(_z, -180, 180);
            EditorGUILayout.EndHorizontal();
            if (Mathf.Abs(prevZ - _z) > 0.0001f)
            {
                SceneView view = GetWindow<SceneView>();
                view.GetType().GetMethod("OnGUI", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, null);
            }
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= RotateCamera;
        }

        void RotateCamera(SceneView sceneview)
        {
            Quaternion angle = Quaternion.AngleAxis(_z, Vector3.forward);
            if (sceneview.camera.transform.localRotation != angle)
            {
                sceneview.isRotationLocked = false;
                sceneview.camera.transform.localRotation = angle;
                sceneview.camera.Render();
            }
        }
    }
}