using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterWind : SavableMonoBehaviour, ICuttable
{//2018-01-25: copied from GravityZone

    public Vector2 windVector;//direction
    public float windForce = 10;//magnitude

    private BoxCollider2D coll;
    private RaycastHit2D[] rch2dStartup = new RaycastHit2D[Utility.MAX_HIT_COUNT];

    // Use this for initialization
    void Start()
    {
        coll = GetComponent<BoxCollider2D>();
        if (windVector == Vector2.zero)
        {
            windVector = transform.up;
        }
        windVector.Normalize();
    }
    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "windVector", windVector,
            "windForce", windForce
            );
        set
        {
            windVector = value.Vector2("windVector");
            windForce = value.Float("windForce");
        }
    }
    public override bool IsSpawnedObject => true;

    public override string PrefabName => "AfterWind";

    public bool Cuttable => true;

    void FixedUpdate()
    {
        //Decrease push force as the zone fades
        Vector2 pushVector = windVector * windForce;

        //Push objects in zone
        int count = Utility.Cast(coll, Vector2.zero, rch2dStartup);
        for (int i = 0; i < count; i++)
        {
            Rigidbody2D rb2d = rch2dStartup[i].collider.gameObject.GetComponent<Rigidbody2D>();
            if (rb2d)
            {
                GravityAccepter ga = rb2d.gameObject.GetComponent<GravityAccepter>();
                if (ga)
                {
                    if (!ga.AcceptsGravity)
                    {
                        continue;
                    }
                }
                //Reduce velocity in whatever direction it's moving
                rb2d.velocity -= rb2d.velocity * 0.9f * Time.fixedDeltaTime;
                //Reinforce movement in intended direction
                rb2d.velocity += pushVector;
                if (rb2d.velocity.magnitude > windForce)
                {
                    rb2d.velocity = rb2d.velocity.normalized * windForce;
                }
            }
        }
    }

    public void cut(Vector2 start, Vector2 end)
    {
        Managers.Object.destroyObject(gameObject);
    }
}
