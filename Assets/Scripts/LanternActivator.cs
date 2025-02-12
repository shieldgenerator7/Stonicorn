using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternActivator : MemoryMonoBehaviour
{

    /// <summary>
    /// The sound that plays when this lantern is lit.
    /// </summary>
    public AudioClip lightSound;

    /// <summary>
    /// The hidden area to reveal when this lantern is activated
    /// </summary>
    public HiddenArea secretHider;

    [AutoInitialize(SearchChildren = true), SerializeField, HideInInspector]
    private List<ParticleSystem> particleSystemList;

    protected override void nowDiscovered()
    {
        foreach (ParticleSystem ps in particleSystemList)
        {
            ps.Play();
        }
        Managers.Sound.playSound(lightSound, transform.position);
        //Hidden Area
        if (secretHider)
        {
            secretHider.Discovered = true;
        }
        //delete this script
        Destroy(this);
    }

    protected override void previouslyDiscovered()
    {
        foreach (ParticleSystem ps in particleSystemList)
        {
            ps.Play();
        }
        Destroy(this);//delete this script
    }
}
