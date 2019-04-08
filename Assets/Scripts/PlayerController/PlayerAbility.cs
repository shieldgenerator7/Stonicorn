using UnityEngine;
using System.Collections;

public class PlayerAbility : MonoBehaviour
{
    public Color effectColor;//the color used for the particle system upon activation

    public ParticleSystemController abilityIndicatorParticleController;
    public ParticleSystemController effectParticleController;
    private ParticleSystem effectParticleSystem;
    public bool addsOnTeleportVisualEffect = true;
    public AudioClip soundEffect;
    public bool addsOnTeleportSoundEffect = true;

    protected PlayerController playerController;
    protected Rigidbody2D rb2d;

    // Use this for initialization
    protected virtual void init()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        if (effectParticleController)
        {
            effectParticleSystem = effectParticleController.GetComponent<ParticleSystem>();
            if (addsOnTeleportVisualEffect)
            {
                if (playerController)
                {
                    playerController.onShowTeleportEffect += showTeleportEffect;
                }
            }
        }
        else
        {
            Debug.LogWarning("PlayerAbility (" + this.GetType() + ") on " + gameObject.getKey() + " does not have a particle effect! effectParticleController: " + effectParticleController);
        }
        if (soundEffect)
        {
            if (addsOnTeleportSoundEffect)
            {
                if (playerController)
                {
                    playerController.onPlayTeleportSound += playTeleportSound;
                }
            }
        }
        if (!abilityIndicatorParticleController)
        {
            throw new System.NullReferenceException(
                "PlayerAbility (" + this.GetType() + ") on " + gameObject.getKey()
                + " does not have a abilityIndicatorParticleController! abilityIndicatorParticleController: " + abilityIndicatorParticleController);
        }
        abilityIndicatorParticleController.activate(true);
    }
    public virtual void OnDisable()
    {
        if (playerController)
        {
            if (addsOnTeleportVisualEffect)
            {
                playerController.onShowTeleportEffect -= showTeleportEffect;
            }
            if (addsOnTeleportSoundEffect)
            {
                playerController.onPlayTeleportSound -= playTeleportSound;
            }
        }
        abilityIndicatorParticleController.activate(false);
    }
    public void OnEnable()
    {
        init();
    }
    
    public virtual void endEffects() { }



    protected void playEffect(Vector2 playPos)
    {
        playEffect(playPos, true);
    }

    protected void playEffect(bool play = true)
    {
        playEffect(effectParticleSystem.transform.position, play);
    }

    protected void playEffect(Vector2 playPos, bool play)
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

    protected virtual void showTeleportEffect(Vector2 oldPos, Vector2 newPos)
    {
        playEffect(oldPos);
    }

    protected virtual void playTeleportSound(Vector2 oldPos, Vector2 newPos)
    {
        SoundManager.playSound(soundEffect, oldPos);
    }

}
