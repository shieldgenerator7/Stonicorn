using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VariableSetAction))]
public class VariableSetTrigger : EventTrigger
{
    public VariableSetAction variableSetAction;
    public VariableSetAction triggerLeaveAction;

    public override int checkForErrors()
    {
        base.checkErrors();
        int errorCount = base.checkForErrors();
        if (!variableSetAction && !triggerLeaveAction)
        {
            Debug.LogError($"VariableSetTrigger doesn't have a variableSetAction or triggerLeaveAction! {variableSetAction}, {triggerLeaveAction}", this);
            errorCount++;
        }
        return errorCount;
    }

    protected override void triggerEvent()
    {
        variableSetAction?.processAllActions();
    }

    //dirty: should be a system in super class for this
    private void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.isPlayerSolid())
        {
            Debug.Log("(dialogue) leaving trigger, activating variable effects", this);
            triggerLeaveAction?.processAllActions();
        }
    }
}
