using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VariableSetAction))]
public class VariableSetTrigger : EventTrigger
{
    public VariableSetAction variableSetAction;

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
}
