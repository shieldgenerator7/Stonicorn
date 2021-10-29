﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSliceAbility : StonicornAbility
{

    [Header("Settings")]
    public int maxAirPorts = 0;//how many times Merky can teleport into the air without being exhausted
    [Header("Components")]
    public StonicornAbility[] excludeAbilityFromGrounding;
    public GameObject cloudPrefab;

    private int airPorts = 0;//"air teleports": how many airports Merky has used since touching the ground
    public int AirPortsUsed
    {
        get => airPorts;
        private set
        {
            airPorts = Mathf.Max(0, value);
            onAirPortsUsedChanged?.Invoke(airPorts, maxAirPorts);
        }
    }
    public delegate void OnAirPortsUsedChanged(int airPortsUsed, int maxAirPorts);
    public event OnAirPortsUsedChanged onAirPortsUsedChanged;

    private SwapAbility swapAbility;

    // Use this for initialization
    public override void init()
    {
        base.init();
        stonicorn.onGroundedStateUpdated += resetAirPorts;
        swapAbility = GetComponent<SwapAbility>();
        GetComponent<ForceLaunchAbility>().onLaunch += useAirPort;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        stonicorn.onGroundedStateUpdated -= resetAirPorts;
        GetComponent<ForceLaunchAbility>().onLaunch -= useAirPort;
    }

    protected override bool isGrounded()
        => (AirPortsUsed < maxAirPorts);

    public bool canReset(GroundChecker grounder) =>
        grounder.isGroundedWithoutAbility(excludeAbilityFromGrounding);

    void resetAirPorts(GroundChecker grounder)
    {
        if (canReset(grounder))
        {
            //Refresh air teleports
            AirPortsUsed = 0;
        }
    }

    void useAirPort()
    {
        AirPortsUsed++;
        if (AirPortsUsed == maxAirPorts)
        {
            stonicorn.updateGroundedState();
        }
    }
    public void grantAirPort()
    {
        if (enabled)
        {
            AirPortsUsed--;
            if (AirPortsUsed < maxAirPorts)
            {
                stonicorn.updateGroundedState();
            }
        }
    }

    protected override void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        if (!stonicorn.Ground.isGroundedPrevWithoutAbility(this)
            && stonicorn.Ground.GroundedPrev)
        {
            //Update Stats
            Managers.Stats.addOne(Stat.AIR_SLICE);
            //Update air ports
            useAirPort();
            //Slice things
            if (CanSlice)
            {
                sliceThings(oldPos, newPos);
                //TODO: Get back
                //Managers.Effect.showTeleportStreak(oldPos, newPos);
            }
            //Cloud
            if (CanMakeCloud)
            {
                makeCloud(oldPos);
            }
            //Give player time to tap again after teleporting in the air
            //Also nullify velocity
            if (AirPortsUsed <= maxAirPorts)
            {
                rb2d.nullifyMovement();
                //TODO: Refactor
                //stonicorn.MovementPaused = true;
            }
            //Effect Teleport
            effectTeleport(oldPos, newPos);
        }
    }

    bool CanSlice
        => FeatureLevel >= 1 &&
        (stonicorn.Ground.GroundedAbility || stonicorn.Ground.GroundedAbilityPrev);

    void sliceThings(Vector2 oldPos, Vector2 newPos)
    {
        //Check if sliced something
        bool slicedSomething = false;
        Utility.RaycastAnswer answer = Utility.RaycastAll(oldPos, (newPos - oldPos), Vector2.Distance(oldPos, newPos));
        for (int i = 0; i < answer.count; i++)
        {
            RaycastHit2D rch2d = answer.rch2ds[i];
            GameObject rchGO = rch2d.collider.gameObject;
            if (rchGO != gameObject
                && rchGO != swapAbility.SwapTarget)
            {
                ICuttable cuttable = rchGO.GetComponent<ICuttable>();
                if (cuttable != null && cuttable.Cuttable)
                {
                    cuttable.cut(oldPos, newPos);
                    slicedSomething = true;
                    Managers.Stats.addOne(Stat.AIR_SLICE_OBJECT);
                }
            }
        }
        //if the player slices something, 
        if (slicedSomething)
        {
            //Allow them to teleport more in the air
            AirPortsUsed--;
        }
    }

    bool CanMakeCloud
        => FeatureLevel >= 2 &&
        (stonicorn.Ground.GroundedAbility || stonicorn.Ground.GroundedAbilityPrev);

    void makeCloud(Vector2 oldPos)
    {
        GameObject cloud = Utility.Instantiate(cloudPrefab);
        cloud.transform.position = oldPos;
        cloud.transform.up = -stonicorn.GravityAccepter.Gravity;
    }

    public override SavableObject CurrentState
    {
        get => base.CurrentState.more(
            "AirPortsUsed", AirPortsUsed
            );
        set
        {
            base.CurrentState = value;
            AirPortsUsed = value.Int("AirPortsUsed");
        }
    }

    protected override void acceptUpgradeLevel(AbilityUpgradeLevel aul)
    {
        maxAirPorts = (int)aul.stat1;
        AirPortsUsed = 0;
    }
}
