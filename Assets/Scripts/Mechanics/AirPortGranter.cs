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
                Managers.Object.destroyObject(gameObject);
            }
            else
            {
                GetComponent<SpriteRenderer>().color =
                    GetComponent<SpriteRenderer>().color.adjustAlpha(1);
            }
        }
    }

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
            Fader fader = GetComponent<Fader>();
            fader.onFadeFinished -= disappear;
            fader.onFadeFinished += disappear;
            fader.enabled = true;
        }
    }

    void disappear()
    {
        Used = true;
    }

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "used", used
            );
        set
        {
            Used = value.Bool("used");
        }
    }
}
