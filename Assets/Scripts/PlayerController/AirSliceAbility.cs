using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSliceAbility : PlayerAbility {

    public float sliceDamage = 100f;//how much force damage to do to objects that get cut
    public GameObject streakPrefab;

    private SwapAbility swapAbility;

	// Use this for initialization
	protected override void init () {
        base.init();
        playerController.maxAirPorts++;
        playerController.onTeleport += sliceThings;
        swapAbility = GetComponent<SwapAbility>();
	}
    public override void OnDisable()
    {
        base.OnDisable();
        playerController.maxAirPorts--;
        playerController.onTeleport -= sliceThings;
    }

    void sliceThings(Vector2 oldPos, Vector2 newPos)
    {
        if (!playerController.GroundedPrev)
        {
            bool slicedSomething = false;
            Utility.RaycastAnswer answer = Utility.RaycastAll(oldPos, (newPos - oldPos), Vector2.Distance(oldPos, newPos));
            for (int i = 0; i < answer.count; i++)
            {
                RaycastHit2D rch2d = answer.rch2ds[i];
                if (rch2d.collider.gameObject != gameObject
                    && !swapAbility.SwappableObjects.Contains(rch2d.collider.gameObject))
                {
                    HardMaterial hm = rch2d.collider.gameObject.GetComponent<HardMaterial>();
                    if (hm)
                    {
                        hm.addIntegrity(-sliceDamage);
                        slicedSomething = true;
                    }
                }
            }
            if (slicedSomething)
            {
                //if the player slices something, allow them to teleport once more in the air
                playerController.AirPortsUsed--;
            }
        }
    }

    protected override void showTeleportEffect(Vector2 oldPos, Vector2 newPos)
    {
        if (!playerController.GroundedPrev)
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
}
