using System;
using UnityEngine;

public class PlayerPilotController : MonoBehaviour
{

    public PlayerController playerController;//the player controller specific to this particular pod
    public CheckPointChecker checkPointChecker;

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
    }

    public void activate(bool active)
    {
        playerController.Teleport.onRangeChanged -= updateRange;
        if (active)
        {
            playerController.Teleport.onRangeChanged += updateRange;
            Managers.Player.Teleport.Range = playerController.Teleport.Range;
            checkPointChecker?.clearPostTeleport(true);
            Managers.PlayerPilot = this;
        }
        else
        {
            Managers.Player.Teleport.Range = Managers.Player.Teleport.baseRange;
            CheckPointChecker.current = null;//dirty: the checkpoint system should handle this better
            checkPointChecker?.trigger();
            if (Managers.PlayerPilot == this)
            {
                Managers.PlayerPilot = null;
            }
        }
    }

    void updateRange(float range)
    {
        Managers.Player.Teleport.Range = range;
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
