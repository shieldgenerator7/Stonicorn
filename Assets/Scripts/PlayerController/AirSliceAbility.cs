using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSliceAbility : PlayerAbility
{

    [Header("Settings")]
    public float sliceDamage = 100f;//how much force damage to do to objects that get cut
    public int maxAirPorts = 0;//how many times Merky can teleport into the air without being exhausted
    [Header("Components")]
    public GameObject streakPrefab;

    private int airPorts = 0;//"air teleports": how many airports Merky has used since touching the ground
    public int AirPortsUsed
    {
        get => airPorts;
        private set => airPorts = Mathf.Max(0, value);
    }

    private SwapAbility swapAbility;

    // Use this for initialization
    protected override void init()
    {
        base.init();
        playerController.Ground.isGroundedCheck += airGroundedCheck;
        playerController.onGroundedStateUpdated += resetAirPorts;
        playerController.onPreTeleport += sliceThings;
        swapAbility = GetComponent<SwapAbility>();
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.Ground.isGroundedCheck -= airGroundedCheck;
        playerController.onGroundedStateUpdated -= resetAirPorts;
        playerController.onPreTeleport -= sliceThings;
    }

    bool airGroundedCheck()
    {
        return AirPortsUsed < maxAirPorts;
    }

    void resetAirPorts(bool grounded, bool groundedNormal)
    {
        if (groundedNormal)
        {
            //Refresh air teleports
            AirPortsUsed = 0;
        }
    }

    void sliceThings(Vector2 oldPos, Vector2 newPos, Vector2 triesPos)
    {
        if (!playerController.Ground.GroundedNormal)
        {
            //Update Stats
            GameStatistics.addOne("AirSlice");
            //Update air ports
            AirPortsUsed++;
            //Check if sliced something
            bool slicedSomething = false;
            Utility.RaycastAnswer answer = Utility.RaycastAll(oldPos, (newPos - oldPos), Vector2.Distance(oldPos, newPos));
            for (int i = 0; i < answer.count; i++)
            {
                RaycastHit2D rch2d = answer.rch2ds[i];
                if (rch2d.collider.gameObject != gameObject
                    && rch2d.collider.gameObject != swapAbility.SwapTarget)
                {
                    HardMaterial hm = rch2d.collider.gameObject.GetComponent<HardMaterial>();
                    if (hm)
                    {
                        hm.addIntegrity(-sliceDamage);
                        slicedSomething = true;
                        GameStatistics.addOne("AirSliceObject");
                    }
                }
            }
            //if the player slices something, 
            if (slicedSomething)
            {
                //Allow them to teleport more in the air
                AirPortsUsed = 0;
            }
            //Give player time to tap again after teleporting in the air
            //Also nullify velocity
            if (AirPortsUsed <= maxAirPorts)
            {
                rb2d.velocity = Vector2.zero;
                playerController.MovementPaused = true;
            }
        }
    }

    protected override void showTeleportEffect(Vector2 oldPos, Vector2 newPos)
    {
        if (!playerController.Ground.GroundedPrev)
        {
            base.showTeleportEffect(oldPos, newPos);
            showStreak(oldPos, newPos);
        }
    }
    void showStreak(Vector3 oldPos, Vector3 newPos)
    {
        GameObject newTS = Instantiate(streakPrefab);
        TeleportStreakUpdater tsu = newTS.GetComponent<TeleportStreakUpdater>();
        tsu.start = oldPos;
        tsu.end = newPos;
        tsu.position();
        tsu.turnOn(true);
    }

    public override SavableObject getSavableObject()
    {
        SavableObject savObj = base.getSavableObject();
        savObj.data.Add(
            "AirPortsUsed", AirPortsUsed
            );
        return savObj;
    }

    public override void acceptSavableObject(SavableObject savObj)
    {
        base.acceptSavableObject(savObj);
        AirPortsUsed = (int)savObj.data["AirPortsUsed"];
    }

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        maxAirPorts = (int)aul.stat1;
    }
}
