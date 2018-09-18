using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemController : MonoBehaviour {
    //2017-03-09 used to control an object with a ParticleSystem thats used for circular ranges

    private ParticleSystem teleportParticles;
    public bool dependsOnTeleportRange = false;//true if it changes size when the teleport range changes

    private bool activated = false;

    // Use this for initialization
    void Awake ()
    {
        teleportParticles = GetComponent<ParticleSystem>();
        activateTeleportParticleSystem(false);
    }

    private void OnEnable()
    {
        //Refresh
        activate(activated);
    }

    /// <summary>
    /// Sets the active variable and tells the particle system whether or not to play
    /// </summary>
    /// <param name="active"></param>
    public void activate(bool active)
    {
        activated = active;
        if (active)
        {
            if (!teleportParticles)
            {
                Debug.LogError("GameObject: " + gameObject.name);
            }
            teleportParticles.Play();
        }
        else
        {
            teleportParticles.Pause();
            teleportParticles.Clear();
        }
    }

    public void activateTeleportParticleSystem(bool activate)
    {
        activateTeleportParticleSystem(activate, teleportParticles.main.startColor.color, transform.position, teleportParticles.shape.radius);
    }
    /// <summary>
    /// Activates the gesture particle system (used mainly for force wave)
    /// </summary>
    /// <param name="activate"></param>
    /// <param name="effectColor"></param>
    /// <param name="pos">Position in world coordinates, not local coordinates</param>
    /// <param name="radius"></param>
    public void activateTeleportParticleSystem(bool activate, Color effectColor, Vector3 pos, float radius)
    {
        this.activate(activate);
        if (activate)
        {
            //Position
            teleportParticles.transform.position = pos;
            //Range
            setRange(radius, false);
            //Color
            ParticleSystem.MainModule psmm = teleportParticles.main;
            psmm.startColor = effectColor;
            //Lifetime
            ParticleSystem.MinMaxCurve psmmc = teleportParticles.main.startLifetime;
            psmmc.constant = radius / Mathf.Abs(teleportParticles.main.startSpeed.constant);
            psmm.startLifetime = psmmc;
        }
    }
    /// <summary>
    /// For activating a radial system that emits outward (for the teleport range indicator)
    /// </summary>
    /// <param name="activate"></param>
    /// <param name="radius">The radius to set to, 0 to do no change</param>
    public void activateTeleportParticleSystem(bool activate, float radius)
    {
        this.activate(activate);
        if (activate)
        {
            //Range
            if (radius > 0)
            {
                setRange(radius, true);
            }
        }

    }
    public void setRange(float newRange, bool andRate)
    {
        ParticleSystem.ShapeModule pssm = teleportParticles.shape;
        if (pssm.radius != newRange)
        {
            pssm.radius = newRange;
            if (andRate)//whether or not to change the rate too
            {
                //Number of particles
                //2017-03-14 I know it's deprecated but it doesn't give me any other option
                teleportParticles.emissionRate = newRange * 100 / 3;
                //Reset
                if (teleportParticles.isPlaying)
                {
                    teleportParticles.Stop();
                    teleportParticles.Play();
                }
            }
        }
    }
    /// <summary>
    /// Sets the range so that the particles die when they reach the given newRange
    /// </summary>
    /// <param name="newRange">The range at which the particles will die</param>
    public void setOuterRange(float newRange)
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        float distanceCoverable = teleportParticles.main.startLifetime.constant * teleportParticles.main.startSpeed.constant;
        setRange(newRange - distanceCoverable, true);
    }
}
