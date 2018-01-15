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
    public float maxSpeedBoost = 2;//how much force to give Merky when teleporting in a direction

    public float currentCharge = 0;//how much charge it has
    public float chargeIncrement = 0.1f;//how much to increment the charge by each teleport
    public float maxCharge = 1;//the maximum amount of charge possible
    public float minChargeDecayDelay = 0.25f;//how much time (sec) of idleness before the charge starts decreasing
    public float maxChargeDecayDelay = 2.0f;
    public float chargeDecayRate = 0.4f;//how much charge decays per sec of idleness (after chargeDecayDelay)

    private float lastTeleportTime;
    public AudioClip forceTeleportSound;
    private Rigidbody2D rb2d;

    protected override void Start()
    {
        base.Start();
        if (playerController)
        {
            playerController.onPreTeleport += charge;
            playerController.onTeleport += giveSpeedBoost;
            rb2d = GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (currentCharge > 0)
        {
            processHoldGesture(transform.position, currentCharge, false);
            if (Time.time > lastTeleportTime + minChargeDecayDelay
                && Time.time > lastTeleportTime + minChargeDecayDelay + ((maxChargeDecayDelay - minChargeDecayDelay) * currentCharge/maxCharge))
            {
                currentCharge -= chargeDecayRate * Time.deltaTime;
                if (currentCharge < 0)
                {
                    currentCharge = 0;
                    dropHoldGesture();
                }
            }
        }
    }

    public void giveSpeedBoost(Vector2 oldPos, Vector2 newPos)
    {
        if (currentCharge > 0)
        {
            Vector2 force = (newPos - oldPos) * maxSpeedBoost * (currentCharge - chargeIncrement);
            RaycastHit2D[] rch2ds = Physics2D.RaycastAll(oldPos, newPos - oldPos, Vector2.Distance(oldPos, newPos));
            foreach (RaycastHit2D rch2d in rch2ds)
            {
                Rigidbody2D orb2d = rch2d.collider.gameObject.GetComponent<Rigidbody2D>();
                if (orb2d)
                {
                    orb2d.AddForce(force * orb2d.mass);
                }
            }
        }
    }

    public void charge(Vector2 oldPos, Vector2 newPos, Vector2 triedPos)
    {
        if ((newPos-triedPos).sqrMagnitude < 0.25f //0.5f * 0.5f
            //If there's a blastable in range, explode instead of charge
            || !isBlastableInArea(triedPos, getRangeFromCharge(currentCharge) / 2))
        {
            currentCharge += chargeIncrement * Vector2.Distance(oldPos, newPos) / playerController.baseRange;
            if (currentCharge > maxCharge)
            {
                currentCharge = maxCharge;
            }
            lastTeleportTime = Time.time;
        }
        else
        {
            processHoldGesture(triedPos, Mathf.Max(currentCharge, chargeIncrement), true);
            currentCharge = 0;
            dropHoldGesture();
        }
    }

    public override void processHoldGesture(Vector2 pos, float holdTime, bool finished)
    {
        float range = getRangeFromCharge(holdTime);
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
                foreach (Blastable b in hitColliders[i].gameObject.GetComponents<Blastable>())
                {
                    float force = forceAmount * (range - b.getDistanceFromExplosion(pos)) / Time.fixedDeltaTime;
                    b.checkForce(force);
                }
            }
            showExplosionEffect(pos, range * 2);
            AudioSource.PlayClipAtPoint(forceTeleportSound, pos);
            Destroy(frii);
            frii = null;
            particleController.activateTeleportParticleSystem(false);
            //EffectManager.clearForceWaveShadows();
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
            //Force Wave Shadows
            //Collider2D[] hitColliders = Physics2D.OverlapCircleAll(pos, range);
            //for (int i = 0; i < hitColliders.Length; i++)
            //{
            //    Rigidbody2D orb2d = hitColliders[i].gameObject.GetComponent<Rigidbody2D>();
            //    if (orb2d != null)
            //    {
            //        EffectManager.showForceWaveShadows(pos, range, hitColliders[i].gameObject);
            //    }
            //}
        }
    }

    private float getRangeFromCharge(float charge)
    {
        float range = maxRange * charge / maxCharge;
        if (range > maxRange)
        {
            range = maxRange;
        }
        return range;
    }

    public override void dropHoldGesture()
    {
        if (frii != null)
        {
            Destroy(frii);
            frii = null;
        }
        particleController.activateTeleportParticleSystem(false);
        //EffectManager.clearForceWaveShadows();
    }

    /// <summary>
    /// Returns whether or not an object with a Blastable or RigidBody component
    /// is within the given range of the given position
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    bool isBlastableInArea(Vector2 pos, float range)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(pos, range);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            Rigidbody2D orb2d = hitColliders[i].gameObject.GetComponent<Rigidbody2D>();
            if (orb2d != null)
            {
                return true;
            }
            Blastable b = hitColliders[i].gameObject.GetComponent<Blastable>();
            if (b != null)
            {
                return true;
            }
        }
        return false;
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
