using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerActivatorTrigger : ActivatorTrigger
{
    public bool forPlayerOnly = true;

    private bool triggered = false;
    public override bool Triggered => triggered;

    private void OnTriggerEnter2D(Collider2D coll2d)
    {
        if (!forPlayerOnly || coll2d.isPlayerSolid())
        {
            triggered = true;
            triggeredChanged();
        }
    }

    private void OnTriggerExit2D(Collider2D coll2d)
    {
        if (!forPlayerOnly || coll2d.isPlayerSolid())
        {
            triggered = false;
            triggeredChanged();
        }
    }
}
