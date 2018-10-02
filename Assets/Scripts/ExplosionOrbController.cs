using UnityEngine;
using System.Collections;

public class ExplosionOrbController : SavableMonoBehaviour
{

    public int forceAmount = 300;
    public bool destroyedOnLastCharge = true;//when this orb runs out of charges, it gets destroyed
    private CircleCollider2D cc2D;
    private ForceTeleportAbility fta;

    private float chargeTime = 0.0f;//amount of time spent charging
    private int chargesLeft = 1;//how many charges it has left

    [Header("Tutorial toggles")]
    public bool explodesUponContact = true;//false = requires mouseup (tap up) to explode
    public bool chargesAutomatically = true;//false = requires hold to charge
    public bool explodesAtAll = true;//false = doesn't do anything

    private RaycastHit2D[] rch2dsTrigger = new RaycastHit2D[Utility.MAX_HIT_COUNT];

    // Use this for initialization
    void Start()
    {
        cc2D = GetComponent<CircleCollider2D>();
        fta = GetComponent<ForceTeleportAbility>();
        HardMaterial hm = GetComponent<HardMaterial>();
        hm.shattered += destroyed;
    }

    // Update is called once per frame
    void Update()
    {
        if (explodesAtAll && chargesLeft > 0)
        {

            if (explodesUponContact)
            {
                bool validTrigger = false;
                Utility.RaycastAnswer answer = Utility.Cast(cc2D, Vector2.zero, rch2dsTrigger, 0, true);
                for (int i = 0; i < answer.count; i++)
                {
                    RaycastHit2D rch2d = answer.rch2ds[i];
                    if (!rch2d.collider.isTrigger
                        && rch2d.collider.gameObject.GetComponent<Rigidbody2D>() != null)
                    {
                        validTrigger = true;
                        break;
                    }
                }
                if (validTrigger)
                {
                    Debug.Log("Collision!");
                    if (chargeTime >= fta.maxCharge)
                    {
                        trigger();
                    }
                }
            }
            if (chargesAutomatically)
            {
                charge(Time.deltaTime);
            }
        }
        else
        {
            if (fta.isHoldingGesture())
            {
                fta.dropHoldGesture();
            }
        }
    }

    public void charge(float deltaChargeTime)
    {
        chargeTime += deltaChargeTime;
        fta.processHoldGesture(transform.position, chargeTime, false);
    }

    public void trigger()
    {
        if (chargesLeft > 0)
        {
            fta.processHoldGesture(transform.position, chargeTime, true);
            chargeTime = 0;
            setChargesLeft(chargesLeft - 1);
        }
        else
        {
            if (destroyedOnLastCharge)
            {
                HardMaterial hm = GetComponent<HardMaterial>();
                hm.addIntegrity(-hm.maxIntegrity);
            }
        }
    }
    private void setChargesLeft(int charges)
    {
        chargesLeft = charges;
        if (chargesLeft > 0)
        {
            GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f);
        }
        else
        {
            GetComponent<SpriteRenderer>().color = new Color(0.25f, 0.25f, 0.25f);
        }
    }

    //When this object's HardMaterial breaks, it will explode
    private void destroyed()
    {
        chargeTime = fta.maxCharge;
        trigger();
        fta.dropHoldGesture();
    }

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this, "chargesLeft", chargesLeft, "chargeTime", chargeTime);
    }
    public override void acceptSavableObject(SavableObject savObj)
    {
        chargesLeft = (int)savObj.data["chargesLeft"];
        setChargesLeft(chargesLeft);
        chargeTime = (float)savObj.data["chargeTime"];
    }

}
