using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers a dialogue cutscene
/// </summary>
[NonSolid]
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
        //don't start a new dialogue if one is already active
        if (Managers.Event.DialoguePlaying)
        {
            Debug.Log($"(dialogue) not triggering because theres something already playing", this);
            Managers.Event.OnDialoguePlayingChanged -= queueTrigger;
            Managers.Event.OnDialoguePlayingChanged += queueTrigger;
            return;
        }
        //
        variableSetAction?.processAllActions();
        Managers.Event.processEventTrigger(this);
    }

    void queueTrigger(bool playing)
    {
        if (!playing)
        {
            Debug.Log($"(dialogue) ok playing done, will trigger now", this);
            Managers.Event.OnDialoguePlayingChanged -= queueTrigger;
            triggerEvent();
        }
    }
}
