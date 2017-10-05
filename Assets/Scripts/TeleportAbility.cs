using UnityEngine;
using System.Collections;

/// <summary>
/// Currently just used for the teleport hold gesture effect,
/// but might actually have teleport capabilities in the future
/// </summary>
public class TeleportAbility : PlayerAbility
{//2017-08-07: copied from ForceTeleportAbility
    public GameObject teleportRangeIndicator;//prefab
    private TeleportRangeIndicatorUpdater friu;//"force range indicator updater"
    private GameObject frii;//"force range indicator instance"
    public GameObject futureProjection;//the object that is used to show a preview of the landing spot
    
    public float maxRange = 3;
    public float maxHoldTime = 1;//how long until the max range is reached

    private PlayerController pc;//reference to the player controller for teleport stuff

    protected override void Start()
    {
        base.Start();
        pc = GetComponent<PlayerController>();
    }

    public new bool takesGesture()
    {
        return true;
    }

    public new bool takesHoldGesture()
    {
        return true;
    }

    public override void processHoldGesture(Vector2 pos, float holdTime, bool finished)
    {
        float range = maxRange * holdTime * GestureManager.holdTimeScaleRecip / maxHoldTime;
        if (range > maxRange)
        {
            range = maxRange;
        }
        if (finished)
        {
            futureProjection.SetActive(false);
            Destroy(frii);
            frii = null;
            particleController.activateTeleportParticleSystem(false);
            if (circularProgressBar != null)
            {
                circularProgressBar.setPercentage(0);
            }
        }
        else
        {
            futureProjection.SetActive(true);
            futureProjection.transform.rotation = transform.rotation;
            futureProjection.transform.localScale = transform.localScale;
            Vector2 futurePos = pc.findTeleportablePosition(pos);
            futureProjection.transform.position = futurePos;
            if (frii == null)
            {
                frii = Instantiate(teleportRangeIndicator);
                friu = frii.GetComponent<TeleportRangeIndicatorUpdater>();
                frii.GetComponent<SpriteRenderer>().enabled = false;
            }
            frii.transform.position = futurePos;
            friu.setRange(range);
            //Particle effects
            particleController.activateTeleportParticleSystem(true, effectColor, futurePos, range);
            if (circularProgressBar != null)
            {
                circularProgressBar.setPercentage(range / maxRange);
                circularProgressBar.transform.position = pos;
            }
        }
    }

    public override void dropHoldGesture()
    {
        futureProjection.SetActive(false);
        if (frii != null)
        {
            Destroy(frii);
            frii = null;
        }
        particleController.activateTeleportParticleSystem(false);
        if (circularProgressBar != null)
        {
            circularProgressBar.setPercentage(0);
        }
    }
}
