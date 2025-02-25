using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBarCircular : MonoBehaviour
{//2017-07-29: copied from AbilityGainEffect

    [AutoInitialize, SerializeField, HideInInspector]
    private new ParticleSystem particleSystem;
    [SerializeField, HideInInspector]
    private float originalEmission;
    [SerializeField, HideInInspector]
    private float originalArc = 0;
    [SerializeField, HideInInspector]
    private Quaternion originalQuat;

    [SerializeField, HideInInspector]
    private float arcEmissionRatio;

    // Use this for initialization
    void Start()
    {
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
        ParticleSystem.EmissionModule psem = particleSystem.emission;
        psem.rateOverTime = newArc * arcEmissionRatio;
        particleSystem.gameObject.transform.localRotation = Quaternion.Euler(
                originalQuat.eulerAngles + new Vector3(0, 0, originalArc - newArc)
                );
    }

    [Initializer]
    private float init_originalEmission => particleSystem.emission.rateOverTime.constant;
    [Initializer]
    private float init_originalArc => particleSystem.shape.arc;
    [Initializer]
    private Quaternion init_originalQuat => particleSystem.gameObject.transform.localRotation;
    [Initializer]
    private float init_arcEmissionRatio => originalEmission / originalArc;
}

