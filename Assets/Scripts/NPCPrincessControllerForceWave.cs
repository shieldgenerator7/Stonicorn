using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCPrincessControllerForceWave : NPCPrincessController {

    public GameObject objectToMove;//the boulder that needs to be moved
    public Vector2 posToMoveTo;//the position of the breakable wall that needs to be broken by the boulder

    protected override void Update()
    {
        Vector2 boulderPos = objectToMove.transform.position;
        offsetVector = boulderPos + ((boulderPos - posToMoveTo).normalized * 0.5f) - (Vector2)transform.position;
        base.Update();
    }
}
