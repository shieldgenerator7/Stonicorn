#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTestSpawnPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("PlayerTestSpawnPoint activating");
        //Set player position
        FindObjectOfType<PlayerController>().transform.position = transform.position;
        //Activate abilities
        foreach (MilestoneActivator maa in GetComponents<MilestoneActivator>())
        {
            if (maa.enabled)
            {
                maa.activateEffect();
            }
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
