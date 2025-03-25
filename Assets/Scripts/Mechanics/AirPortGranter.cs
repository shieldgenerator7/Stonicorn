using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirPortGranter : SavableMonoBehaviour
{
    bool used = false;
    bool Used
    {
        get => used;
        set
        {
            used = value;
            if (used)
            {
                Managers.Object.destroyObject(SavableObjectInfo);
            }
            else
            {
                sr.color = sr.color.adjustAlpha(1);
            }
        }
    }

    [AutoInitialize, SerializeField, HideInInspector]
    private SpriteRenderer sr;

    [AutoInitialize, SerializeField, HideInInspector]
    private Fader fader;

    public override void init()
    {
    }

    private void OnTriggerEnter2D(Collider2D coll2d)
    {
        if (coll2d.isSolid())
        {
            AirSliceAbility asa = coll2d.gameObject.GetComponent<AirSliceAbility>();
            if (asa)
            {
                asa.grantAirPort();
            }
            //fade
            fader.onFadeFinished -= disappear;
            fader.onFadeFinished += disappear;
            fader.enabled = true;
        }
    }

    void disappear()
    {
        Used = true;
    }

    static short key_used = 0;
    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            key_used, used
            );
        set
        {
            Used = value.Bool(key_used);
        }
    }
}
