using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollectTrigger : EventTrigger
{
    [Tooltip("The title of the dialogue to play")]
    public string title;
    [Tooltip("The variable to increment by 1 if there is no title")]
    public string counterName;

    protected override void Start()
    {
        //If this item has already been collected,
        if (FindObjectOfType<DialogueManager>().progressManager.hasActivated(this))
        {
            //Destroy it
            destroy();
        }

        //Parent start up process
        base.Start();

        //Item must have a title or a counterName
        if ((title == null || title == "")
            && (counterName == null || counterName == ""))
        {
            Debug.LogError(
                "ItemCollectTrigger must have either a title or a counterName."
                , this);
        }
    }

    protected override void triggerEvent()
    {
        //Find dialogue path by its title
        if (title != "" && title != null)
        {
            dialogueManager.playDialogue(title);
        }
        else
        {
            dialogueManager.progressManager.add(counterName);
        }
        destroy();
    }

    private void destroy()
    {
        Destroy(this);
        Destroy(transform.parent.gameObject);
    }
}
