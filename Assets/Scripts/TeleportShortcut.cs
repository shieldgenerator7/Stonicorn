using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportShortcut : MonoBehaviour
{

    //used to skip parts of the game to facilitate testing

    public Vector2 teleportLocation;//where to teleport Merky to
    public List<MilestoneActivator> abilitiesToGrant;//a list of abilities to grant Merky when he goes through this skip

    void Start()
    {
        if (abilitiesToGrant == null)
        {
            abilitiesToGrant = new List<MilestoneActivator>();
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.isPlayer())
        {
            coll.gameObject.transform.position = teleportLocation;
            foreach (MilestoneActivator ma in abilitiesToGrant)
            {
                if (ma != null)
                {
                    ma.Discovered = true;
                }
            }
        }
    }
}
