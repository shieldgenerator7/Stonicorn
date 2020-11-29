using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternActivator : MemoryMonoBehaviour
{

    /// <summary>
    /// The sound that plays when this lantern is lit.
    /// </summary>
    public AudioClip lightSound;

    protected override void nowDiscovered()
    {
        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }
        Managers.Sound.playSound(lightSound, transform.position);
        Destroy(this);//delete this script
    }

    protected override void previouslyDiscovered()
    {
        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }
        Destroy(this);//delete this script
    }
}
