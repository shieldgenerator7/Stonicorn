#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTestSpawnPoint : MonoBehaviour
{
    public bool ForceLaunchAbility;
    public bool SwapAbility;
    public bool WallClimbAbility;
    public bool AirSliceAbility;
    public bool ElectricRingAbility;
    public bool LongTeleportAbility;

    private void Awake()
    {
        Managers.DemoMode.DemoMode = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("PlayerTestSpawnPoint activating");
        //Set player position
        PlayerController pc = FindObjectOfType<PlayerController>();
        pc.transform.position = transform.position;
        //Activate abilities
        if (ForceLaunchAbility)
        {
            pc.GetComponent<ForceLaunchAbility>().enabled = true;
        }
        if (SwapAbility)
        {
            pc.GetComponent<SwapAbility>().enabled = true;
        }
        if (WallClimbAbility)
        {
            pc.GetComponent<WallClimbAbility>().enabled = true;
        }
        if (AirSliceAbility)
        {
            pc.GetComponent<AirSliceAbility>().enabled = true;
        }
        if (ElectricRingAbility)
        {
            pc.GetComponent<ElectricRingAbility>().enabled = true;
        }
        if (LongTeleportAbility)
        {
            pc.GetComponent<LongTeleportAbility>().enabled = true;
        }
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
}

#endif
