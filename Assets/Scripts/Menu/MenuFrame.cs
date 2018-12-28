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
    void Start()
    {
        bc2d = GetComponent<BoxCollider2D>();
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
        CameraController camcon = cam.GetComponent<CameraController>();

        //Set camera orthographic size
        Vector3 camTop = Camera.main.ViewportToWorldPoint(new Vector3(0, 1));
        Vector3 camBot = Camera.main.ViewportToWorldPoint(new Vector3(0, 0));
        float camWorldHeight = Vector3.Distance(camTop, camBot);
        float curOrthoSize = cam.orthographicSize;
        camcon.TargetZoomLevel = bc2d.bounds.size.y * curOrthoSize / camWorldHeight;

        //Set camera position
        //(by using CameraController.Offset)
        Vector3 offset = camcon.Offset;
        camcon.Offset =
            transform.position
            - GameManager.getPlayerObject().transform.position
            + new Vector3(0, 0, offset.z);
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
}
