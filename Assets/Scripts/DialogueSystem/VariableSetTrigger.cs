using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VariableSetAction))]
public class VariableSetTrigger : EventTrigger
{
    public VariableSetAction variableSetAction;
    public VariableSetAction triggerLeaveAction;

    protected override void checkErrors()
    {
        base.checkErrors();
        if (!variableSetAction)
        {
            Debug.LogError($"VariableSetTrigger doesn't have a variableSetAction! {variableSetAction}");
        }
    }

    protected override void triggerEvent()
    {
        variableSetAction.processAllActions();
    }

    //dirty: should be a system in super class for this
    private void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.isPlayerSolid())
        {
            triggerLeaveAction?.processAllActions();
        }
    }
}
