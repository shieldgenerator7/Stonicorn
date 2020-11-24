using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenArea : MemoryMonoBehaviour
{
    //The class is just here so that the Hidden Areas themselves
    //remember whether they've been found or not,
    //and not their triggers

    public bool discovered = false;

    //2016-11-26: called when this HiddenArea has just been discovered now
    public void nowDiscovered()
    {
        discovered = true;
        Managers.Game.saveMemory(this);
        Fader fader = gameObject.AddComponent<Fader>();
        fader.ignorePause = true;
    }

    //2016-11-26: called when this HiddenArea had been discovered in a previous session
    public void previouslyDiscovered()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (!coll.isTrigger && coll.gameObject.isPlayer())
        {
            nowDiscovered();
        }
    }

    public override MemoryObject getMemoryObject()
    {
        return new MemoryObject(this, discovered);
    }
    public override void acceptMemoryObject(MemoryObject memObj)
    {
        if (memObj.found)
        {
            previouslyDiscovered();
        }
    }
}
