using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Physics2DSurrogate : MonoBehaviour
{
    private List<GravityZone> gravityZones;
    private MusicZone[] musicZones;
 
    private void OnEnable()
    {
        Physics2D.autoSimulation = false;
        SceneManager.sceneLoaded += refreshZones;
        SceneManager.sceneUnloaded += refreshZones;
        refreshZones();
    }

    private void OnDisable()
    {
        Physics2D.autoSimulation = true;
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
    void refreshZones() { 
        //Gravity
        gravityZones = new List<GravityZone>();
        foreach (GravityZone gz in FindObjectsOfType<GravityZone>())
        {
            if (gz.mainGravityZone)
            {
                gravityZones.Add(gz);
            }
        }
        //Music
        musicZones = FindObjectsOfType<MusicZone>();
    }

    private void Update()
    {
        //Camera
        CameraController cam = Managers.Camera;
        cam.refocus();
        Vector2 camPos = cam.transform.position;
        //Gravity
        foreach(GravityZone gz in gravityZones)
        {
            if (gz.GetComponent<Collider2D>().OverlapPoint(camPos))
            {
                cam.transform.up = gz.transform.up;
                break;
            }
        }
        //Music
        foreach (MusicZone mz in musicZones)
        {
            if (mz.GetComponent<Collider2D>().OverlapPoint(camPos))
            {
                mz.playTrack();
                break;
            }
        }
    }
}
