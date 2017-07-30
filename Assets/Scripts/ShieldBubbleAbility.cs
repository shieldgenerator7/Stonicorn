using UnityEngine;
using System.Collections;

public class ShieldBubbleAbility : PlayerAbility
{//2017-01-17: copied from ForceTeleportAbility

    public const bool ALLOW_SHIELD_STACKING = true;//whether or not shields are allowed to intersect each other

    public GameObject shieldRangeIndicator;//prefab
    private TeleportRangeIndicatorUpdater sriu;//"shield range indicator updater"
    private GameObject srii;//"shield range indicator instance"
    public GameObject shieldBubblePrefab;//prefab used to spawn shield bubbles
    public float maxRange = 2.5f;
    public float maxHoldTime = 1;//how long until the max range is reached
    public bool spawnDemos = false;//if true, spawns shields that don't do anything and that fade slowly

    public AudioClip shieldBubbleSound;
    public AudioClip shieldBubbleFailSound;//played when a shield cannot be made because it overlaps another shield

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
        GameObject go;//not used
        processHoldGesture(pos, holdTime, finished, out go);
    }
    public void processHoldGesture(Vector2 pos, float holdTime, bool finished, out GameObject spawnedShield)
    {
        spawnedShield = null;
        float range = maxRange * holdTime * GestureManager.holdTimeScaleRecip / maxHoldTime;
        if (range > maxRange)
        {
            range = maxRange;
        }
        if (finished)
        {
            //Spawn Shield Bubble
            if (canSpawnShieldBubble(pos, range))
            {
                GameObject newSB = spawnShieldBubble(pos, range, 100 * range / maxRange);
                if (!spawnDemos)
                {
                    GameManager.addObject(newSB);
                }
                spawnedShield = newSB;
                AudioSource.PlayClipAtPoint(shieldBubbleSound, pos);
            }
            else
            {
                AudioSource.PlayClipAtPoint(shieldBubbleFailSound, pos);
            }
            Destroy(srii);
            srii = null;
            particleController.activateTeleportParticleSystem(false);
            if (circularProgressBar != null)
            {
                circularProgressBar.setPercentage(0);
            }
        }
        else {
            if (srii == null)
            {
                srii = Instantiate(shieldRangeIndicator);
                sriu = srii.GetComponent<TeleportRangeIndicatorUpdater>();
                srii.GetComponent<SpriteRenderer>().enabled = false;
            }
            srii.transform.position = (Vector2)pos;
            sriu.setRange(range);
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
        if (srii != null)
        {
            Destroy(srii);
            srii = null;
        }
        particleController.activateTeleportParticleSystem(false);
        if (circularProgressBar != null)
        {
            circularProgressBar.setPercentage(0);
        }
    }

    public bool canSpawnShieldBubble(Vector2 pos, float range)
    {
        if (ALLOW_SHIELD_STACKING)
        {
            return true;
        }
        if (spawnDemos)
        {
            return true;
        }
        //Check to make sure it won't overlap another shield bubble
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(pos, range);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.GetComponent<ShieldBubbleController>() != null)
            {
                //found another shield bubble
                return false;
            }
        }
        return true;
    }

    public GameObject spawnShieldBubble(Vector2 pos, float range, float energy)
    {
        //Spawn New Shield Bubble
        GameObject newSB = (GameObject)Instantiate(shieldBubblePrefab);
        newSB.transform.position = pos;
        newSB.GetComponent<ShieldBubbleController>().init(range, energy);
        newSB.name += System.DateTime.Now.Ticks;
        if (spawnDemos)
        {
            newSB.GetComponent<EdgeCollider2D>().enabled = false;
            newSB.AddComponent<Fader>();
        }
        return newSB;
    }
}
