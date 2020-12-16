using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class PlayerAbility : SavableMonoBehaviour, ISetting
{
    //the color used for the particle system upon activation
    public Color EffectColor => teleportRangeSegment.color;

    public TeleportRangeSegment teleportRangeSegment;
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
            Active = unlocked;
        }
    }
    public bool Active
    {
        get => enabled;
        set
        {
            if (enabled != value)
            {
                enabled = value;
            }
        }
    }

    [Header("Persisting Variables")]
    [SerializeField]
    [Range(0, 6)]
    private int upgradeLevel = 0;
    public List<AbilityUpgradeLevel> upgradeLevels;
    public int UpgradeLevel
    {
        get => upgradeLevel;
        set
        {
            upgradeLevel = Mathf.Max(
                upgradeLevel,
                Mathf.Clamp(value, 0, upgradeLevels.Count - 1)
                );
            acceptUpgradeLevel(upgradeLevel);
        }
    }

    protected PlayerController playerController;
    protected Rigidbody2D rb2d;

    // Use this for initialization
    protected virtual void init()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        //Upgrade Levels
        acceptUpgradeLevel(upgradeLevel);

        if (playerController)
        {
            //Sound Effects
            if (soundEffect)
            {
                if (addsOnTeleportSoundEffect)
                {
                    playerController.onPlayTeleportSound += playTeleportSound;
                }
            }
            playerController.abilityActivated(this, true);
            //Delegates
            playerController.Teleport.onTeleport += processTeleport;
            playerController.Ground.isGroundedCheck += isGrounded;
        }
    }
    public virtual void OnDisable()
    {
        if (playerController)
        {
            if (addsOnTeleportSoundEffect)
            {
                playerController.onPlayTeleportSound -= playTeleportSound;
            }
            playerController.abilityActivated(this, false);
            //Delegates
            playerController.Teleport.onTeleport -= processTeleport;
            playerController.Ground.isGroundedCheck -= isGrounded;
        }
    }
    public void OnEnable()
    {
        init();
    }

    protected abstract bool isGrounded();
    protected abstract void processTeleport(Vector2 oldPos, Vector2 newPos);
    /// <summary>
    /// To be called by subtypes after they have influenced a teleport
    /// </summary>
    /// <param name="oldPos"></param>
    /// <param name="newPos"></param>
    protected void effectTeleport(Vector2 oldPos, Vector2 newPos)
    {
        onEffectedTeleport?.Invoke(oldPos, newPos);
    }
    public event TeleportAbility.OnTeleport onEffectedTeleport;

    public virtual void stopGestureEffects() { }

    private void acceptUpgradeLevel(int level)
    {
        if (upgradeLevels.Count > 0)
        {
            acceptUpgradeLevel(upgradeLevels[level]);
        }
    }
    protected abstract void acceptUpgradeLevel(AbilityUpgradeLevel aul);

    protected int FeatureLevel
        => upgradeLevels[upgradeLevel].featureLevel;

    protected virtual void playTeleportSound(Vector2 oldPos, Vector2 newPos)
    {
        Managers.Sound.playSound(soundEffect, oldPos);
    }

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "upgradeLevel", upgradeLevel
            );
        set => UpgradeLevel = value.Int("upgradeLevel");
    }

    public SettingScope Scope => SettingScope.SAVE_FILE;

    public string ID => GetType().Name;

    public SettingObject Setting
    {
        get =>
            new SettingObject(ID,
                "unlocked", unlocked,
                "upgradeLevel", upgradeLevel
                );
        set
        {
            Unlocked = (bool)value.data["unlocked"] || unlocked;
            UpgradeLevel = (int)value.data["upgradeLevel"];
        }
    }

#if UNITY_EDITOR
    public void testUpgradeLevel()
    {
        acceptUpgradeLevel(upgradeLevel);
    }
#endif

}
