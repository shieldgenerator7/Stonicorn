using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers a dialogue cutscene
/// </summary>
public class DialogueTrigger : EventTrigger
{
    public List<string> characters;

    public override bool Interactable
    {
        get
        {
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
        //Find dialogue path by its title
        if (HasTitle)
        {
            Managers.Event.playDialogue(title);
        }
        //Find dialogue path by characters
        else
        {
            Managers.Event.playDialogue(characters);
        }
    }
}
