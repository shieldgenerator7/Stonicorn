using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines an area for the camera to see a certain area of the menu screen
/// </summary>
public class MenuFrame : MonoBehaviour
{
    public int scalePoint = 0;

    private List<MenuButton> buttons = new List<MenuButton>();

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
        float widthRatio = cam.pixelWidth / bc2d.bounds.size.x;
        float heightRatio = cam.pixelHeight / bc2d.bounds.size.y;
        CameraController camcon = cam.GetComponent<CameraController>();
        camcon.TargetZoomLevel = camcon.scalePointToZoomLevel(scalePoint);

        Vector3 offset = camcon.Offset;
        camcon.Offset =
            transform.position
            - GameManager.getPlayerObject().transform.position
            + new Vector3(0, 0, offset.z);
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
                mb.activate();
                return;
            }
        }
    }
}
