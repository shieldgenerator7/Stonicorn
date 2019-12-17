using UnityEngine;

public class ForceBoostAbility : PlayerAbility
{//2019-04-06: copied from ForceTeleportAbility

    [Header("Settings")]
    public float forceAmount = 10;//how much force to apply = forceAmount * 2^(holdTime*10)
    public float maxForce = 1000;//the maximum amount of force applied to one object
    public float maxRange = 3;
    public float maxSpeedBoost = 2;//how much force to give Merky when teleporting in a direction
    public float wakelessSpeedBoostMultiplier = 3;//how much to multiply the speed boost by when there's no wake

    private float prevCharge = 0;
    public float Charge
    {
        get
        {
            float charge = (playerController.Speed / maxSpeed) * maxCharge;
            charge = Mathf.Clamp(charge, 0, maxCharge);
            return charge;
        }
    }
    public float chargeClutch = 0.2f;//what percentage of the speed converts to charge
    public float chargeIncrement = 0.1f;//how much to increment the charge by each teleport
    public float maxCharge = 1;//the maximum amount of charge possible
    public float maxSpeed = 15;//after this is reached, charge amount per speed remains constant

    public float minWindDuration = 0.3f;
    public float maxWindDuration = 0.5f;

    private float lastTeleportTime;

    [Header("Components")]
    public GameObject forceRangeIndicator;//prefab
    private TeleportRangeIndicatorUpdater friu;//"force range indicator updater"
    public GameObject explosionEffect;
    public GameObject afterWindPrefab;//the prefab for the temporary windzone this ability creates
    public AudioClip forceTeleportSound;

    protected override void init()
    {
        base.init();
        if (playerController)
        {
            playerController.onPreTeleport += charge;
            playerController.onTeleport += giveSpeedBoost;
        }
    }
    public override void OnDisable()
    {
        base.OnDisable();
        if (playerController)
        {
            playerController.onPreTeleport -= charge;
            playerController.onTeleport -= giveSpeedBoost;
        }
    }

    private float maxSpeedAttained = 0;
    private void Update()
    {
        float charge = Charge;
        if (Mathf.Floor(playerController.Speed) > maxSpeedAttained)
        {
            maxSpeedAttained = Mathf.Floor(playerController.Speed);
            Debug.Log("Player Speed: " + maxSpeedAttained);
        }
        if (charge > 0)
        {
            doExplosionEffect(transform.position, charge, false);
        }
        if (charge <= 0)
        {
            dropHoldGesture();
        }
    }

    private void FixedUpdate()
    {
        float charge = Charge;
        charge = Mathf.Floor(charge * 100) / 100;
        if (charge > 0)
        {
            prevCharge = charge;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!Active)
        {
            return;
        }
        //If collision is head on,
        Vector2 velocity = playerController.Velocity;
        Vector2 surfaceNormal = collision.GetContact(0).normal;
        float angle = Utility.RotationZ(-velocity, surfaceNormal);
        if (angle < 45)
        {
            //Explode     
            Vector2 explodePos;
            //If object is breakable or movable,
            if (collision.gameObject.isSavable())
            {
                //explode behind Merky
                explodePos = (Vector2)transform.position - (velocity.normalized * 0.01f);
            }
            //Else
            else
            {
                //explode in front of Merky  
                explodePos = (Vector2)transform.position - (Vector2.Reflect(velocity, surfaceNormal).normalized * 0.01f);
            }
            Vector2 dir = ((Vector2)transform.position - explodePos).normalized;
            Debug.DrawLine(explodePos, explodePos + (dir * 2), Color.white, 2);
            float charge = Mathf.Max(prevCharge, Charge);
            prevCharge = 0;
            doExplosionEffect(explodePos, Mathf.Max(charge, chargeIncrement), true);
            dropHoldGesture();
        }
        //Else
        else
        {
            float speed = playerController.Speed;
            //Divert Merky's course
            //If should rotate "left"
            if (Vector2.SignedAngle(-velocity, surfaceNormal) < 0)
            {
                rb2d.velocity = surfaceNormal.normalized.PerpendicularRight() * speed;
            }
            else
            {
                rb2d.velocity = surfaceNormal.normalized.PerpendicularLeft() * speed;
            }
            ////Add a bit of force to make up for friction
            //rb2d.AddForce(rb2d.velocity.normalized * chargeIncrement);
        }
    }

    public void giveSpeedBoost(Vector2 oldPos, Vector2 newPos)
    {
        if (Charge > 0)
        {
            float magnitude = (newPos - oldPos).magnitude;
            Vector2 force = (newPos - oldPos).normalized * maxSpeedBoost * (Charge / maxCharge) * (magnitude / playerController.baseRange);
            //If the player uses Long Teleport, make a wake
            if (magnitude > playerController.baseRange + 0.5f)
            {
                GameObject afterWind = Utility.Instantiate(afterWindPrefab);
                afterWind.transform.up = force.normalized;
                afterWind.transform.position = oldPos;
                afterWind.transform.localScale = new Vector3(1, magnitude, 1);
                AfterWind aw = afterWind.GetComponent<AfterWind>();
                aw.windVector = force;
                aw.fadeOutDuration = minWindDuration + ((maxWindDuration - minWindDuration) * Charge / maxCharge);
                //Update Stats
                GameStatistics.addOne("ForceChargeWake");
            }
            else
            {
                //Push the player and all objects in the teleport path
                Utility.RaycastAnswer answer = Utility.RaycastAll(oldPos, newPos - oldPos, Vector2.Distance(oldPos, newPos));
                for (int i = 0; i < answer.count; i++)
                {
                    RaycastHit2D rch2d = answer.rch2ds[i];
                    Rigidbody2D orb2d = rch2d.collider.gameObject.GetComponent<Rigidbody2D>();
                    if (orb2d)
                    {
                        if (orb2d.velocity.magnitude <= maxSpeed)
                        {
                            orb2d.AddForce(force * wakelessSpeedBoostMultiplier);
                        }
                    }
                }
                //Update Stats
                GameStatistics.addOne("ForceChargeBoost");
            }
        }
    }

    public void charge(Vector2 oldPos, Vector2 newPos, Vector2 triedPos)
    {
        float range = getRangeFromCharge(Charge);
        if (Mathf.Approximately(Charge, 0))
        {
            //The first teleport will only make the charge increase a small amount, no matter how far it was
            rb2d.AddForce((newPos - oldPos).normalized * chargeIncrement);
        }
        else
        {
            rb2d.AddForce((newPos - oldPos).normalized * chargeIncrement * Vector2.Distance(oldPos, newPos) / playerController.baseRange);
        }
        lastTeleportTime = Time.time;
    }



    public void doExplosionEffect(Vector2 pos, float holdTime, bool finished)
    {
        float range = getRangeFromCharge(holdTime);
        if (finished)
        {
            //Make the blast
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(pos, range);
            for (int i = 0; i < hitColliders.Length; i++)
            {
                GameObject hitGO = hitColliders[i].gameObject;
                Rigidbody2D orb2d = hitGO.GetComponent<Rigidbody2D>();
                if (orb2d != null)
                {
                    orb2d.AddWeightedExplosionForce(forceAmount, pos, range, maxForce);
                }
                foreach (Blastable b in hitGO.GetComponents<Blastable>())
                {
                    if (hitGO != playerController.gameObject)
                    {
                        float force = forceAmount * (range - b.getDistanceFromExplosion(pos)) / Time.fixedDeltaTime;
                        b.checkForce(force);
                    }
                }
            }
            showExplosionEffect(pos, range * 2);
            Managers.Sound.playSound(forceTeleportSound, pos);
            dropHoldGesture();
            //Update Stats
            GameStatistics.addOne("ForceChargeBlast");
        }
        else
        {
            if (friu == null)
            {
                GameObject frii = Instantiate(forceRangeIndicator);
                friu = frii.GetComponent<TeleportRangeIndicatorUpdater>();
                frii.GetComponent<SpriteRenderer>().enabled = false;
            }
            friu.transform.position = (Vector2)pos;
            friu.setRange(range);
            //Particle effects
            effectParticleController.activateTeleportParticleSystem(true, effectColor, pos, range);
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
        if (friu != null)
        {
            Destroy(friu.gameObject);
            friu = null;
        }
        effectParticleController.activateTeleportParticleSystem(false);
    }

    void showExplosionEffect(Vector2 pos, float finalSize)
    {
        GameObject newTS = (GameObject)Instantiate(explosionEffect);
        ExplosionEffectUpdater eeu = newTS.GetComponent<ExplosionEffectUpdater>();
        eeu.start = pos;
        eeu.finalSize = finalSize;
        eeu.position();
        eeu.init();
        eeu.turnOn(true);
    }
}
