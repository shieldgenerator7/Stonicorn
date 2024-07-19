using System;
using UnityEngine;

public class PlayerPilotController : MonoBehaviour
{

    public PlayerController playerController;//the player controller specific to this particular pod

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void init()
    {
        playerController.init();
        playerController.Teleport.onTeleport += (oldPos, newPos) =>
        {
            Managers.Player.transform.position = newPos;
        };

        playerController.Teleport.onRangeChanged += (range) => Managers.Player.Teleport.Range = range;
    }

    public void activate(bool active)
    {
        if (active)
        {
            Managers.Player.Teleport.Range = playerController.Teleport.Range;
        }
        else
        {
            Managers.Player.Teleport.Range = Managers.Player.Teleport.baseRange;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Processes the tap gesture at the given position
    /// </summary>
    /// <param name="curMPWorld">The position of the tap in world coordinates</param>
    public void processTapGesture(Vector3 curMPWorld)
    {
        playerController.processTapGesture(curMPWorld);

        //Process tapProcessed delegates
        tapProcessed?.Invoke(curMPWorld);
    }
    public delegate void TapProcessed(Vector2 curMPWorld);
    public event TapProcessed tapProcessed;

    public void processHoldGesture(Vector3 holdPos, float holdTime, bool finished)
    {
        playerController.processHoldGesture(holdPos, holdTime, finished);
    }

    public void processDragGesture(Vector3 origPos, Vector3 newPos, bool finished)
    {
        playerController.processDragGesture(origPos, newPos, finished);
    }
}
