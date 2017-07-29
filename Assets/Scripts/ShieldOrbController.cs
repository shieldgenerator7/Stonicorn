using UnityEngine;
using System.Collections;

public class ShieldOrbController : MonoBehaviour
{
    //2017-03-13: copied from ExplosionOrbController
    private CircleCollider2D cc2D;
    private ShieldBubbleAbility sba;
    private RaycastHit2D[] rh2ds = new RaycastHit2D[5];//used in Update() for a method call. Not actually updated

    private float chargeTime = 0.0f;//amount of time spent charging
    private GameObject currentSB;//the current shield this orb has made, used to make sure only one shield is active per orb at a time

    //Tutorial toggles
    public bool generatesUponContact = true;//false = requires mouseup (tap up) to explode
    public bool chargesAutomatically = true;//false = requires hold to charge
    public bool generatesAtAll = true;//false = doesn't do anything

    // Use this for initialization
    void Start()
    {
        cc2D = GetComponent<CircleCollider2D>();
        sba = GetComponent<ShieldBubbleAbility>();
    }

    // Update is called once per frame
    void Update()
    {
        if (generatesAtAll)
        {
            if ((currentSB == null || ReferenceEquals(currentSB, null)) && sba.canSpawnShieldBubble(transform.position, sba.maxRange))
            {
                if (chargesAutomatically)
                {
                    charge(Time.deltaTime);
                }
                if (generatesUponContact)
                {
                    if (chargeTime >= sba.maxHoldTime && isBeingTriggered())
                    {
                        trigger();
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (generatesAtAll)
        {
            if ((currentSB == null || ReferenceEquals(currentSB, null)) && sba.canSpawnShieldBubble(transform.position, sba.maxRange))
            {
                if (generatesUponContact)
                {
                    if (chargeTime >= sba.maxHoldTime)
                    {
                        trigger();
                    }
                }
            }
            else
            {
                Debug.Log("Can't generate because shield already exists. SOC: " + gameObject.name);
            }
        }
    }

    public bool isBeingTriggered()
    {
        int count = cc2D.Cast(Vector2.zero, rh2ds, 0, true);
        if (count <= 0) { return false; }
        for (int i=0; i<count; i++)
        {
            if (!rh2ds[i].collider.isTrigger)
            {
                return true;
            }
        }
        return false;
    }

    public void charge(float deltaChargeTime)
    {
        chargeTime += deltaChargeTime;
        sba.processHoldGesture(transform.position, chargeTime, false);
    }

    public void trigger()
    {
        //Shield Orbs require to be at full capacity to deploy a shield
        if (sba.maxHoldTime <= chargeTime)
        {
            GameObject newSB;
            sba.processHoldGesture(transform.position, chargeTime, true, out newSB);
            if (newSB != null)
            {
                currentSB = newSB;
            }
            chargeTime = 0;
        }
        sba.dropHoldGesture();
    }

}
