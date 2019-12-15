using UnityEngine;
using System.Collections;

public class PlayerAbility : SavableMonoBehaviour
{
    public Color effectColor;//the color used for the particle system upon activation

    public TeleportRangeSegment teleportRangeSegment;
    public ParticleSystemController effectParticleController;
    private ParticleSystem effectParticleSystem;
    public bool addsOnTeleportVisualEffect = true;
    public AudioClip soundEffect;
    public bool addsOnTeleportSoundEffect = true;

    [Header("Savable Variables")]
    [SerializeField]
    private bool unlocked = false;//whether the player has it available to use
    public bool Unlocked
    {
        get => unlocked;
        set
        {
            unlocked = value;
            if (unlocked)
            {
                Active = true;
            }
        }
    }
    [SerializeField]
    private bool active = false;//whether the player has turned it on or off (default true after unlocked first becomes true)
    public bool Active
    {
        get => active;
        set
        {
            active = value;
            if (active)
            {
                enabled = true;
                init();
            }
        }
    }

    protected PlayerController playerController;
    protected Rigidbody2D rb2d;

    // Use this for initialization
    protected virtual void init()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        if (addsOnTeleportVisualEffect)
        {
            if (effectParticleController)
            {
                effectParticleSystem = effectParticleController.GetComponent<ParticleSystem>();
                if (playerController)
                {
                    playerController.onShowTeleportEffect += showTeleportEffect;
                }
            }
            else
            {
                Debug.LogWarning("PlayerAbility (" + this.GetType() + ") on " + name + " does not have a particle effect! effectParticleController: " + effectParticleController);
            }
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
        if (playerController)
        {
            playerController.abilityActivated(this, true);
        }
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
            playerController.abilityActivated(this, false);
        }
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
        Managers.Sound.playSound(soundEffect, oldPos);
    }

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this,
            "unlocked", unlocked,
            "active", active
            );
    }

    public override void acceptSavableObject(SavableObject savObj)
    {
        unlocked = (bool)savObj.data["unlocked"];
        active = (bool)savObj.data["active"];
        if (unlocked && active)
        {
            enabled = true;
            init();
        }
    }

}
