using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternActivator : MemoryMonoBehaviour {

    /// <summary>
    /// The sound that plays when this lantern is lit.
    /// </summary>
    public AudioClip lightSound;
    public bool lit = false;

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.isPlayer())
        {
            lightTorch();
        }
    }
    
    /// <summary>
    /// Lights the torch
    /// </summary>
    /// <param name="firstTime">True if it has just been activated, false if being reactivated after loading</param>
    void lightTorch(bool firstTime = true)
    {
        lit = true;
        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }
        if (firstTime)
        {
            SoundManager.playSound(lightSound, transform.position);
        }
        Managers.Game.saveMemory(this);
        Destroy(this);//delete this script
    }

    public override MemoryObject getMemoryObject()
    {
        return new MemoryObject(this, lit);
    }
    public override void acceptMemoryObject(MemoryObject memObj)
    {
        if (memObj.found)
        {
            lightTorch(false);
        }
    }
}
