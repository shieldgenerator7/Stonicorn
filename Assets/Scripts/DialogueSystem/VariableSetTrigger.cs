using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableSetTrigger : EventTrigger
{
    public string variableName;
    public int value;

    public bool autoTrigger { get; set; } = true;

    protected override void triggerEvent()
    {
        if (autoTrigger)
        {
            Managers.Progress.set(variableName, value);
        }
    }

    public void triggerEventFromDialogueTrigger(DialogueTrigger trigger)
    {
        if (trigger.variableSetTrigger == this)
        {
            autoTrigger = true;
            triggerEvent();
            autoTrigger = false;
        }
    }
}
