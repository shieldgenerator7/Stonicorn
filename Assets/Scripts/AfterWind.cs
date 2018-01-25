using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterWind : MonoBehaviour
{//2018-01-25: copied from GravityZone

    public Vector2 windVector;//direction and magnitude
    public float fadeOutDuration = 1.0f;//how long (sec) it will take for this to fade away

    private BoxCollider2D coll;
    private SpriteRenderer sr;
    private RaycastHit2D[] rch2dStartup = new RaycastHit2D[100];
    private float fadeStartTime = 0f;//when the fade out started
    private float fadeEndTime = 0f;//when the fade out will end and this GameObject will be deleted

    // Use this for initialization
    void Start()
    {
        coll = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        fadeStartTime = Time.time;
        fadeEndTime = fadeStartTime + fadeOutDuration;
    }

    void FixedUpdate()
    {
        if (GameManager.isRewinding())
        {
            return;//don't do anything if it is rewinding
        }

        //Decrease push force as the zone fades
        float fadeFactor = (fadeEndTime - Time.time) / fadeOutDuration;
        Vector2 pushVector = windVector * fadeFactor;

        //Push objects in zone
        int count = coll.Cast(Vector2.zero, rch2dStartup);
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
                        //Inform the gravity accepter of the direction
                        ga.addGravity(pushVector);
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
