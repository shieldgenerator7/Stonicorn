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
    protected override void init()
    {
        base.init();
        playerController.Ground.isGroundedCheck += airGroundedCheck;
        playerController.onGroundedStateUpdated += resetAirPorts;
        playerController.Teleport.onTeleport += sliceThings;
        swapAbility = GetComponent<SwapAbility>();
    }
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.Ground.isGroundedCheck -= airGroundedCheck;
        playerController.onGroundedStateUpdated -= resetAirPorts;
        playerController.Teleport.onTeleport -= sliceThings;
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

    void sliceThings(Vector2 oldPos, Vector2 newPos)
    {
        if (!playerController.Ground.GroundedNormal)
        {
            //Update Stats
            Managers.Stats.addOne("AirSlice");
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
                        Managers.Stats.addOne("AirSliceObject");
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
                rb2d.nullifyMovement();
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
    }
}
