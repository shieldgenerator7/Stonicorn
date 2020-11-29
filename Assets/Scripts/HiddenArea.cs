using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenArea : MemoryMonoBehaviour
{
    //The class is just here so that the Hidden Areas themselves
    //remember whether they've been found or not,
    //and not their triggers

    //2016-11-26: called when this HiddenArea has just been discovered now
    protected override void nowDiscovered()
    {
        Fader fader = gameObject.AddComponent<Fader>();
        fader.ignorePause = true;
    }

    //2016-11-26: called when this HiddenArea had been discovered in a previous session
    protected override void previouslyDiscovered()
    {
        Destroy(gameObject);
    }
}
