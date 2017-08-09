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
    
    public float maxRange = 3;
    public float maxHoldTime = 1;//how long until the max range is reached

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
            if (frii == null)
            {
                frii = Instantiate(teleportRangeIndicator);
                friu = frii.GetComponent<TeleportRangeIndicatorUpdater>();
                frii.GetComponent<SpriteRenderer>().enabled = false;
            }
            frii.transform.position = (Vector2)pos;
            friu.setRange(range);
            //Particle effects
            particleController.activateTeleportParticleSystem(true, effectColor, pos, range);
            if (circularProgressBar != null)
            {
                circularProgressBar.setPercentage(range / maxRange);
                circularProgressBar.transform.position = pos;
            }
        }
    }

    public override void dropHoldGesture()
    {
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
