using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Physics2DSurrogate : MonoBehaviour
{
    private MusicZone[] musicZones;
    private IEnumerable<GravityZone> gravityZones;

    private void OnEnable()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;//turn off simulation
        SceneManager.sceneLoaded += refreshZones;
        SceneManager.sceneUnloaded += refreshZones;
        refreshZones();
    }

    private void OnDisable()
    {
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;//turn on simulation
        SceneManager.sceneLoaded -= refreshZones;
        SceneManager.sceneUnloaded -= refreshZones;
    }

    void refreshZones(Scene s, LoadSceneMode lsm)
    {
        refreshZones();
    }
    void refreshZones(Scene s)
    {
        refreshZones();
    }
    void refreshZones()
    {
        //Gravity
        gravityZones = FindObjectsByType<GravityZone>(FindObjectsSortMode.None)
            .Where(gz => gz.mainGravityZone);
        //Music
        musicZones = FindObjectsOfType<MusicZone>();
    }

    public void processFrame()
    {
        //Camera
        CameraController cam = Managers.Camera;
        cam.refocus();
        Vector2 camPos = cam.transform.position;
        //Gravity
        GravityZone gz = gravityZones.First(gz => gz.Contains(camPos));
        if (gz)
        {
            cam.transform.up = gz.transform.up;
        }
        //Music
        foreach (MusicZone mz in musicZones)
        {
            if (mz.checkZone(camPos))
            {
                break;
            }
        }
    }
}
