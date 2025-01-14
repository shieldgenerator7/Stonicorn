using UnityEditor;
using UnityEngine;

public class EditorCameraRotatorTool : ToolboxTool
{
    [Range(0, 360)]
    public float rotZ = 0;
    public bool autoRotate = true;

    public override string Name => "Editor Camera Rotator";

    const string PREFS_KEY = "ecr_autorotate";

    protected override void init()
    {
        SceneView.duringSceneGui -= rotateCamera;
        SceneView.duringSceneGui += rotateCamera;
    }

    protected override void initPlayMode()
    {
        SceneView.duringSceneGui -= rotateCamera;
        SceneView.duringSceneGui += rotateCamera;
    }

    protected override void disposeImpl()
    {
        SceneView.duringSceneGui -= rotateCamera;
    }

    public override void display()
    {
        bool newAR = GUILayout.Toggle(autoRotate, "Auto-Rotate");
        if (newAR != autoRotate)
        {
            toggle();
        }
        if (GUILayout.Button("Toggle"))
        {
            toggle();
        }
    }

    void rotateCamera(SceneView sceneview)
    {
        if (autoRotate)
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
                rotZ = sceneview.camera.transform.eulerAngles.z;
            }
        }
        else
        {
            Quaternion angle = Quaternion.AngleAxis(rotZ, Vector3.forward);
            if (sceneview.camera.transform.localRotation != angle)
            {
                sceneview.isRotationLocked = false;
                sceneview.camera.transform.localRotation = angle;
                sceneview.camera.Render();
            }
        }
    }

    public void toggle()
    {
        autoRotate = !autoRotate;
        if (!autoRotate)
        {
            rotZ = 0;
        }
    }

    protected override void save()
    {
        EditorPrefs.SetBool(PREFS_KEY, autoRotate);
    }

    protected override void load()
    {
        autoRotate = EditorPrefs.GetBool(PREFS_KEY,true);
    }
}
