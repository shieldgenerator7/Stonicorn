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
            if (!string.IsNullOrEmpty(title))
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
        if (!string.IsNullOrEmpty(title))
        {
            Managers.Dialogue.playDialogue(title);
        }
        //Find dialogue path by characters
        else
        {
            Managers.Dialogue.playDialogue(characters);
        }
    }
}
