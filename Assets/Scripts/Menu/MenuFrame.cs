using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Defines an area for the camera to see a certain area of the menu screen
/// </summary>
public class MenuFrame : MonoBehaviour, ISetupable
{
    public List<MenuButton> buttons = new List<MenuButton>();

    [AutoInitialize, SerializeField, HideInInspector]
    private BoxCollider2D bc2d;

    public void init()
    {
        buttons.ForEach(button => button.init());
    }

    /// <summary>
    /// Moves the camera to view its frame completely
    /// </summary>
    public void frameCamera()
    {
        Camera cam = Camera.main;
        CameraController camcon = Managers.Camera;

        //Set camera orthographic size
        Vector3 camTop = cam.ViewportToWorldPoint(new Vector3(0, 1, 10));
        Vector3 camBot = cam.ViewportToWorldPoint(new Vector3(0, 0, 10));
        float camWorldHeight = Vector3.Distance(camTop, camBot);
        float curFieldView = cam.fieldOfView;
        camcon.TargetZoomLevel = bc2d.bounds.size.y * curFieldView / camWorldHeight;

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

    public MenuButton findButton(Vector3 tapPos)
    {
        return buttons.FirstOrDefault(mb => mb.tapInArea(tapPos));
    }

    public void delegateTap(Vector3 tapPos)
    {
        buttons.FirstOrDefault(mb => mb.tapInArea(tapPos))?
            .processTap(tapPos);
    }
    public bool delegateDrag(Vector3 origMPWorld, Vector3 newMPWorld, GestureState state)
    {
        foreach (MenuButton mb in buttons)
        {
            if (mb.acceptsDragGesture() && mb.tapInArea(origMPWorld))
            {
                mb.processDrag(newMPWorld, state);
                return true;
            }
        }
        return false;
    }

#if UNITY_EDITOR
    public int setup()
    {
        int changeCount = 0;

        buttons.ForEach(mb => {
                changeCount += mb.setup();
        });

        return changeCount;
    }
#endif
}
