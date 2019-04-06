using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines an area for the camera to see a certain area of the menu screen
/// </summary>
public class MenuFrame : MonoBehaviour
{
    public List<MenuButton> buttons = new List<MenuButton>();

    private BoxCollider2D bc2d;

    // Use this for initialization
    protected virtual void Start()
    {
        init();
    }
    public void init()
    {
        bc2d = GetComponent<BoxCollider2D>();
        buttons.Clear();
        foreach (Transform t in transform)
        {
            MenuButton mb = t.GetComponent<MenuButton>();
            if (mb != null)
            {
                buttons.Add(mb);
            }
        }
    }

    /// <summary>
    /// Moves the camera to view its frame completely
    /// </summary>
    public void frameCamera()
    {
        Camera cam = Camera.main;
        CameraController camcon = Managers.Camera;

        //Set camera orthographic size
        Vector3 camTop = cam.ViewportToWorldPoint(new Vector3(0, 1));
        Vector3 camBot = cam.ViewportToWorldPoint(new Vector3(0, 0));
        float camWorldHeight = Vector3.Distance(camTop, camBot);
        float curOrthoSize = cam.orthographicSize;
        camcon.TargetZoomLevel = bc2d.bounds.size.y * curOrthoSize / camWorldHeight;

        //Set camera position
        //(by using CameraController.Offset)
        camcon.Offset = transform.position - Managers.Player.transform.position;
    }

    public bool canDelegateTaps()
    {
        return buttons != null && buttons.Count > 0;
    }

    public bool tapInArea(Vector3 tapPos)
    {
        return bc2d.OverlapPoint(tapPos);
    }

    public void delegateTap(Vector3 tapPos)
    {
        foreach (MenuButton mb in buttons)
        {
            if (mb.tapInArea(tapPos))
            {
                mb.processTap(tapPos);
                return;
            }
        }
    }
    public bool delegateDrag(Vector3 origMPWorld, Vector3 newMPWorld)
    {
        foreach (MenuButton mb in buttons)
        {
            if (mb.acceptsDragGesture() && mb.tapInArea(origMPWorld))
            {
                mb.processTap(newMPWorld);
                return true;
            }
        }
        return false;
    }
}
