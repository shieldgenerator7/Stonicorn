using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers a dialogue cutscene
/// </summary>
public class DialogueTrigger : EventTrigger
{
    public List<string> characters;

    [Tooltip("The variable actions to take before triggering this dialogue")]
    public VariableSetAction variableSetAction;

    public override bool Interactable
    {
        get
        {
            if (variableSetAction)
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

    protected override void triggerEvent()
    {
        variableSetAction?.processAllActions();
        Managers.Event.processEventTrigger(this);
    }
}
