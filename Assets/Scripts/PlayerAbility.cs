using UnityEngine;
using System.Collections;

public class PlayerAbility : MonoBehaviour {

    GameObject player;
    protected PlayerController playerController;
    public GameObject teleportParticleEffects;
    protected ParticleSystemController particleController;
    protected new ParticleSystem particleSystem;
    public Color effectColor;//the color used for the particle system upon activation

    public GameObject abilityIndicatorParticleEffects;
    public ProgressBarCircular circularProgressBar;

    // Use this for initialization
    protected virtual void Start () {
        player = gameObject;
        playerController = player.GetComponent<PlayerController>();
        particleController = teleportParticleEffects.GetComponent<ParticleSystemController>();
        particleSystem = teleportParticleEffects.GetComponent<ParticleSystem>();
        if (abilityIndicatorParticleEffects != null)
        {
            abilityIndicatorParticleEffects.GetComponent<ParticleSystem>().Play();
        }
    }

    public bool effectsGroundCheck()
    {
        return false;
    }

    public bool effectsAirPorts()
    {
        return false;
    }

    public bool takesGesture()
    {
        return false;
    }

    public bool takesHoldGesture()
    {
        return true;
    }

    public virtual void processHoldGesture(Vector2 pos, float holdTime, bool finished)
    {

    }

    /// <summary>
    /// Returns whether or not this ability has its hold gesture activated
    /// </summary>
    /// <returns></returns>
    public virtual bool isHoldingGesture()
    {
        return particleSystem.isPlaying;
    }

    public virtual void dropHoldGesture() { }

}
