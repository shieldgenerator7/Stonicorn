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

    [Header("Persisting Variables")]
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

    public bool CanUseUltimate => playerController.Teleport.Range > playerController.Teleport.baseRange;

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

    [AutoInitialize, SerializeField, HideInInspector]
    protected PlayerController playerController;
    [AutoInitialize, SerializeField, HideInInspector]
    protected Rigidbody2D rb2d;

    // Use this for initialization
    public override void init()
    {
        //Upgrade Levels
        acceptUpgradeLevel(upgradeLevel);

        //register delegates
        registerDelegatesStart(true);

        //ability activated
        playerController?.abilityActivated(this, true);
    }
    private void registerDelegatesStart(bool register = true)
    {
        if (playerController)
        {
            //Sound Effects
            if (soundEffect)
            {
                if (addsOnTeleportSoundEffect)
                {
                    playerController.onPlayTeleportSound -= playTeleportSound;
                    if (register)
                    {
                        playerController.onPlayTeleportSound += playTeleportSound;
                    }
                }
            }
            //Delegates
            playerController.Teleport.onTeleport -= processTeleport;
            playerController.Ground.isGroundedCheck -= isGrounded;
            if (register)
            {
                playerController.Teleport.onTeleport += processTeleport;
                playerController.Ground.isGroundedCheck += isGrounded;
            }
        }
        registerDelegates(register);
    }
    protected abstract void registerDelegates(bool register = true);
    public void OnDisable()
    {
        registerDelegates(false);
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
        playerController.abilityUpgraded(this, level);
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
        get => new SavableObject(this);
        set { }
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

    public void setUpgradeLevel(int value)
    {
        upgradeLevel = Mathf.Clamp(value, 0, upgradeLevels.Count - 1);
        acceptUpgradeLevel(upgradeLevel);
    }
#endif

}
