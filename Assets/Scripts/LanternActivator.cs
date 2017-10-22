using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternActivator : MemoryMonoBehaviour {

    public bool lit = false;

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == GameManager.playerTag)
        {
            lightTorch();
        }
    }

    void lightTorch()
    {
        lit = true;
        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }
        GameManager.saveMemory(this);
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
            lightTorch();
        }
    }
}
