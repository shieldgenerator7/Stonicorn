using UnityEngine;
using System.Collections;

public class ForceTeleportAbility : PlayerAbility
{
    public GameObject forceRangeIndicator;//prefab
    private TeleportRangeIndicatorUpdater friu;//"force range indicator updater"
    private GameObject frii;//"force range indicator instance"
    public GameObject explosionEffect;

    public float forceAmount = 10;//how much force to apply = forceAmount * 2^(holdTime*10)
    public float maxForce = 1000;//the maximum amount of force applied to one object
    public float maxRange = 3;
    public float maxHoldTime = 1;//how long until the max range is reached

    public AudioClip forceTeleportSound;

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
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(pos, range);
            for (int i = 0; i < hitColliders.Length; i++)
            {
                Rigidbody2D orb2d = hitColliders[i].gameObject.GetComponent<Rigidbody2D>();
                if (orb2d != null)
                {
                    Utility.AddWeightedExplosionForce(orb2d, forceAmount, pos, range, maxForce);
                }
                else
                {
                    HardMaterial hm = hitColliders[i].gameObject.GetComponent<HardMaterial>();
                    if (hm != null)
                    {
                        float force = forceAmount * (range - Utility.distanceToObject(pos, hitColliders[i].gameObject)) / Time.fixedDeltaTime;
                        hm.checkForce(force);
                    }
                    ShieldBubbleController sbc = hitColliders[i].gameObject.GetComponent<ShieldBubbleController>();
                    if (sbc != null)
                    {
                        float force = forceAmount * (range - Utility.distanceToObject(pos, hitColliders[i].gameObject)) / Time.fixedDeltaTime;
                        sbc.checkForce(force);
                    }
                }
            }
            showExplosionEffect(pos, range * 2);
            AudioSource.PlayClipAtPoint(forceTeleportSound, pos);
            Destroy(frii);
            frii = null;
            particleController.activateTeleportParticleSystem(false);
            if (circularProgressBar != null)
            {
                circularProgressBar.setPercentage(0);
            }
            EffectManager.clearForceWaveShadows();
        }
        else
        {
            if (frii == null)
            {
                frii = Instantiate(forceRangeIndicator);
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
            //Force Wave Shadows
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(pos, range);
            for (int i = 0; i < hitColliders.Length; i++)
            {
                Rigidbody2D orb2d = hitColliders[i].gameObject.GetComponent<Rigidbody2D>();
                if (orb2d != null)
                {
                    EffectManager.showForceWaveShadows(pos, range, hitColliders[i].gameObject);
                }
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
        EffectManager.clearForceWaveShadows();
    }



    void showExplosionEffect(Vector2 pos, float finalSize)
    {
        GameObject newTS = (GameObject)Instantiate(explosionEffect);
        newTS.GetComponent<ExplosionEffectUpdater>().start = pos;
        newTS.GetComponent<ExplosionEffectUpdater>().finalSize = finalSize;
        newTS.GetComponent<ExplosionEffectUpdater>().position();
        newTS.GetComponent<ExplosionEffectUpdater>().init();
        newTS.GetComponent<ExplosionEffectUpdater>().turnOn(true);
    }
}
