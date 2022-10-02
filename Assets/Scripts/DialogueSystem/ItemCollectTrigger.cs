using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollectTrigger : EventTrigger
{
    [Tooltip("The variable to increment by 1 if there is no title")]
    public string counterName;

    protected override void checkErrors()
    {
        base.checkErrors();

        //Item must have a title or a counterName
        if (!HasTitle && string.IsNullOrEmpty(counterName))
        {
            Debug.LogError(
                "ItemCollectTrigger must have either a title or a counterName.",
                this
                );
        }
    }

    protected override void previouslyDiscovered()
    {
        destroy();
    }

    protected override void triggerEvent()
    {
        //Find dialogue path by its title
        if (HasTitle)
        {
            Managers.Event.processEventTrigger(this);
        }
        else
        {
            Managers.Progress.add(counterName);
        }
        destroy();
    }

    private void destroy()
    {
        Destroy(this);
        Destroy(transform.parent.gameObject);
    }
}
