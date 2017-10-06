using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityRangeTutorialDisengager : MonoBehaviour
{//2017-10-05: copied from AbilityGainEffect
    public Vector2 disengagePoint;//when Merky gets close enough to this point, the anim will stop
    public float disengageRange = 5.0f;
    private new ParticleSystem particleSystem;

    // Use this for initialization
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        particleSystem.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(transform.position, disengagePoint) <= disengageRange)
        {
            disengage();
        }
    }
    void disengage()
    {
        particleSystem.Stop();
        Destroy(this);
    }
}
