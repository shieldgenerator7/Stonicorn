#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTestSpawnPoint : MonoBehaviour
{
    [Range(-1, 6)]
    public int ForceLaunchAbility = -1;
    [Range(-1, 6)]
    public int SwapAbility = -1;
    [Range(-1, 6)]
    public int WallClimbAbility = -1;
    [Range(-1, 6)]
    public int AirSliceAbility = -1;
    [Range(-1, 6)]
    public int ElectricBeamAbility = -1;
    [Range(-1, 6)]
    public int LongTeleportAbility = -1;

    // Start is called before the first frame update
    public void init()
    {
        Debug.Log("PlayerTestSpawnPoint activating");
        //Set player position
        PlayerController pc = Managers.Player;
        pc.transform.position = transform.position;
        Physics2D.SyncTransforms();
        //Activate abilities
        checkAbility(ForceLaunchAbility, pc.GetComponent<ForceLaunchAbility>());
        checkAbility(SwapAbility, pc.GetComponent<SwapAbility>());
        checkAbility(WallClimbAbility, pc.GetComponent<WallClimbAbility>());
        checkAbility(AirSliceAbility, pc.GetComponent<AirSliceAbility>());
        checkAbility(ElectricBeamAbility, pc.GetComponent<ElectricBeamAbility>());
        checkAbility(LongTeleportAbility, pc.GetComponent<LongTeleportAbility>());

        //Destroy object
        //If it's under a ruler displayer,
        RulerDisplayer rd = GetComponentInParent<RulerDisplayer>();
        if (rd)
        {
            //Destroy that ruler displayer and everything under it
            Destroy(rd.gameObject);
        }
        else
        {
            //Otherwise just destroy this spawn point object
            Destroy(gameObject);
        }
    }
    void checkAbility(int level, PlayerAbility ability)
    {
        if (level >= 0)
        {
            ability.enabled = true;
            ability.UpgradeLevel = level;
        }
    }
}


#endif
