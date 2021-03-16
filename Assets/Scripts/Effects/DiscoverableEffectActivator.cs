using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoverableEffectActivator : MonoBehaviour
{
    public MemoryMonoBehaviour mmb;
    public List<Fader> effects;

    private void Start()
    {
        mmb.onDiscovered += activateEffects;
    }

    private void activateEffects()
    {
        //Start the fade-in effect
        foreach (Fader f in effects)
        {
            f.enabled = true;
        }
    }
}
