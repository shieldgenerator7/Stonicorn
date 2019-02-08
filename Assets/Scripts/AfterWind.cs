using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterWind : SavableMonoBehaviour
{//2018-01-25: copied from GravityZone

    public Vector2 windVector;//direction and magnitude
    public float fadeOutDuration = 1.0f;//how long (sec) it will take for this to fade away

    private BoxCollider2D coll;
    private SpriteRenderer sr;
    private RaycastHit2D[] rch2dStartup = new RaycastHit2D[Utility.MAX_HIT_COUNT];
    private float fadeStartTime = 0f;//when the fade out started
    private float fadeEndTime = 0f;//when the fade out will end and this GameObject will be deleted

    // Use this for initialization
    void Start()
    {
        coll = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        init();
    }
    private void init()
    {
        if (Mathf.Approximately(fadeStartTime, 0))
        {
            fadeStartTime = Time.time;
        }
        fadeEndTime = fadeStartTime + fadeOutDuration;
    }
    public override SavableObject getSavableObject()
    {//2018-02-22: copied from ElectricFieldController.getSavableObject()
        return new SavableObject(this,
            "windVector", windVector,
            "fadeOutDuration", fadeOutDuration,
            "fadeTime", (Time.time - fadeStartTime)
            );
    }
    public override void acceptSavableObject(SavableObject savObj)
    {
        windVector = (Vector2)savObj.data["windVector"];
        fadeOutDuration = (float)savObj.data["fadeOutDuration"];
        fadeStartTime = Time.time - (float)savObj.data["fadeTime"];
        init();
    }
    public override bool isSpawnedObject()
    {
        return true;
    }
    public override string getPrefabName()
    {
        return "ForceChargeAfterWind";
    }

    void FixedUpdate()
    {
        if (GameManager.Rewinding)
        {
            return;//don't do anything if it is rewinding
        }

        //Decrease push force as the zone fades
        float fadeFactor = (fadeEndTime - Time.time) / fadeOutDuration;
        Vector2 pushVector = windVector * fadeFactor;

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
                    if (ga.AcceptsGravity)
                    {
                        rb2d.AddForce(pushVector);
                    }
                }
                else
                {
                    rb2d.AddForce(pushVector);
                }
            }
        }
        //Fade the sprite
        Color prevColor = sr.color;
        sr.color = new Color(prevColor.r, prevColor.g, prevColor.b, Mathf.SmoothStep(0, 1, fadeFactor));
        if (fadeFactor <= 0 || Mathf.Approximately(fadeFactor,0))
        {
            GameManager.destroyObject(gameObject);
        }
    }
}
