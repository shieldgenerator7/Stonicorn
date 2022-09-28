using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers a dialogue cutscene
/// </summary>
public class DialogueTrigger : EventTrigger
{
    public string title;
    public List<string> characters;

    public override bool Interactable
    {
        get
        {
            //Find dialogue path by its title
            if (title != "" && title != null)
            {
                return dialogueManager.hasDialogue(title);
            }
            //Find dialogue path by characters
            else
            {
                return dialogueManager.hasDialogue(characters);
            }
        }
    }

    protected override void triggerEvent()
    {
        //Find dialogue path by its title
        if (title != "" && title != null)
        {
            dialogueManager.playDialogue(title);
        }
        //Find dialogue path by characters
        else
        {
            dialogueManager.playDialogue(characters);
        }
    }
}
