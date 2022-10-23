using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers a dialogue cutscene
/// </summary>
[DisallowMultipleComponent]
public class DialogueTrigger : EventTrigger
{
    public List<string> characters;

    public VariableSetTrigger variableSetTrigger { get; private set; }

    public override bool Interactable
    {
        get
        {
            if (variableSetTrigger)
            {
                return true;
            }
            //Find dialogue path by its title
            if (HasTitle)
            {
                return Managers.Dialogue.hasDialogue(title);
            }
            //Find dialogue path by characters
            else
            {
                return Managers.Dialogue.hasDialogue(characters);
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        variableSetTrigger = GetComponent<VariableSetTrigger>();
        if (variableSetTrigger)
        {
            variableSetTrigger.autoTrigger = false;
        }
    }

    protected override void triggerEvent()
    {
        variableSetTrigger?.triggerEventFromDialogueTrigger(this);
        Managers.Event.processEventTrigger(this);
    }
}
