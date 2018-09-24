using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBarCircular : MonoBehaviour
{//2017-07-29: copied from AbilityGainEffect
    
    private new ParticleSystem particleSystem;
    private float originalEmission;
    private float originalArc = 0;
    private Quaternion originalQuat;

    private float arcEmissionRatio;

    // Use this for initialization
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        originalEmission = particleSystem.emission.rateOverTime.constant;
        originalArc = particleSystem.shape.arc;
        originalQuat = particleSystem.gameObject.transform.localRotation;
        arcEmissionRatio = originalEmission / originalArc;
        setArc(0);
    }

    /// <summary>
    /// Sets the arc of the particle effects to match the percentage
    /// </summary>
    /// <param name="percentage">A number between 0 and 1</param>
    public void setPercentage(float percentage)
    {
        setArc(percentage * 360);
        if (percentage <= 0)
        {
            particleSystem.Stop();
            particleSystem.Clear();
        }
        else if (!particleSystem.isPlaying)
        {
            particleSystem.Play();
        }
    }

    void setArc(float newArc)
    {
        ParticleSystem.ShapeModule pssm = particleSystem.shape;
        pssm.arc = newArc;
        particleSystem.emissionRate = newArc * arcEmissionRatio;
        particleSystem.gameObject.transform.localRotation = Quaternion.Euler(originalQuat.eulerAngles + new Vector3(0, 0, originalArc - newArc));
    }
}

