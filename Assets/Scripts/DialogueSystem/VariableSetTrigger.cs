using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableSetTrigger : EventTrigger
{
    public string variableName;
    public int value;

    protected override void triggerEvent()
    {
        Managers.Progress.set(variableName, value);
    }
}
