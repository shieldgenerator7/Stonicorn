using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityGainEffect : MonoBehaviour
{

    public float animSpeed = 0;//arc degrees per second
    public float animTime = 1.0f;//how many seconds to complete one section of anim
    public Vector2 disengagePoint;//when Merky gets close enough to this point, the anim will stop
    public float disengageRange = 5.0f;
    public ParticleSystem abilityRangeIndicator;//the particles that show the range of the ability, if applicable

    private new ParticleSystem particleSystem;
    private float originalEmission;
    private float originalArc;
    private float originalStartLifetime;
    private Quaternion originalQuat;
    private int originalSpriteOrder;

    private float arcEmissionRatio;
    private bool isDisengaging = false;//true when the anim should wind down

    // Use this for initialization
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        originalEmission = particleSystem.emission.rateOverTime.constant;
        originalArc = particleSystem.shape.arc;
        originalQuat = particleSystem.gameObject.transform.localRotation;
        originalStartLifetime = particleSystem.main.startLifetime.constant;
        originalSpriteOrder = particleSystem.GetComponent<Renderer>().sortingOrder;
        arcEmissionRatio = originalEmission / originalArc;
        setArc(0);
        if (animSpeed == 0)
        {
            animSpeed = 360 / animTime;
        }
        //Start Lifetime
        ParticleSystem.MainModule psmm = particleSystem.main;
        psmm.startLifetime = 0.1f;
        //Sorting Order
        particleSystem.GetComponent<Renderer>().sortingOrder = originalSpriteOrder + 1;
    }

    // Update is called once per frame
    void Update()
    {
        float currentArc = particleSystem.shape.arc;
        setArc(particleSystem.shape.arc + animSpeed * Time.deltaTime);
        if (particleSystem.shape.arc <= 0 || particleSystem.shape.arc >= 360)
        {
            animSpeed *= -1;
        }
        if (isDisengaging)
        {
            if (animSpeed < 0 && Mathf.Abs(originalArc - currentArc) < 10.0f)
            {
                disengage();
            }
        }
        else
        {
            if (Vector2.Distance(transform.position, disengagePoint) <= disengageRange)
            {
                isDisengaging = true;
            }
        }
    }

    void setArc(float newArc)
    {
        ParticleSystem.ShapeModule pssm = particleSystem.shape;
        pssm.arc = newArc;
        ParticleSystem.EmissionModule psem = particleSystem.emission;
        psem.rateOverTime = newArc * arcEmissionRatio * 100;
        particleSystem.gameObject.transform.localRotation = Quaternion.Euler(
                originalQuat.eulerAngles + new Vector3(0, 0, originalArc - newArc)
                );
    }
    void disengage()
    {
        setArc(originalArc);
        ParticleSystem.EmissionModule psem = particleSystem.emission;
        psem.rateOverTime = originalEmission;
        ParticleSystem.MainModule psmm = particleSystem.main;
        psmm.startLifetime = originalStartLifetime;
        particleSystem.GetComponent<Renderer>().sortingOrder = originalSpriteOrder;
        if (abilityRangeIndicator)
        {
            abilityRangeIndicator.Stop();
            abilityRangeIndicator.Clear();
        }
        Destroy(this);
    }
}
