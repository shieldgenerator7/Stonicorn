using UnityEngine;
using System.Collections;

public class PlayerAbility : MonoBehaviour
{
    public Color effectColor;//the color used for the particle system upon activation

    public ParticleSystemController abilityIndicatorParticleController;
    public ParticleSystemController effectParticleController;
    private ParticleSystem effectParticleSystem;

    protected PlayerController playerController;
    protected Rigidbody2D rb2d;

    // Use this for initialization
    protected virtual void init()
    {
        playerController = GetComponent<PlayerController>();
        rb2d = GetComponent<Rigidbody2D>();
        if (effectParticleController)
        {
            effectParticleSystem = effectParticleController.GetComponent<ParticleSystem>();
        }
        else
        {
            Debug.LogWarning("PlayerAbility (" + this.GetType() + ") on " + name + " does not have a particle effect! effectParticleController: " + effectParticleController);
        }
        abilityIndicatorParticleController.activate(true);
    }
    public virtual void OnDisable()
    {
        abilityIndicatorParticleController.activate(false);
    }
    public void OnEnable()
    {
        init();
    }

    public virtual void processHoldGesture(Vector2 pos, float holdTime, bool finished) { }

    /// <summary>
    /// Returns whether or not this ability has its hold gesture activated
    /// </summary>
    /// <returns></returns>
    public virtual bool isHoldingGesture()
    {
        return effectParticleSystem.isPlaying;
    }

    public virtual void dropHoldGesture() { }



    protected void playEffect(Vector2 playPos)
    {
        playEffect(effectParticleSystem.transform.position, true);
    }

    protected void playEffect(bool play = true)
    {
        playEffect(effectParticleSystem.transform.position, play);
    }

    protected void playEffect(Vector2 playPos, bool play = true)
    {
        effectParticleSystem.transform.position = playPos;
        if (play)
        {
            effectParticleSystem.Play();
        }
        else
        {
            effectParticleSystem.Pause();
            effectParticleSystem.Clear();
        }
    }

}
