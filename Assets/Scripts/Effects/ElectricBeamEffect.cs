using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBeamEffect : MonoBehaviour
{
    public ElectricBeamAbility electricBeamAbility;

    SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
        electricBeamAbility.onActivatedChanged += processActivated;
        electricBeamAbility.onTargetChanged += updateStaticEffect;
        processActivated(electricBeamAbility.Activated);
        updateStaticEffect(null, electricBeamAbility.Target);
    }

    // Update is called once per frame
    void Update()
    {
        if (electricBeamAbility.Target)
        {
            sr.enabled = true;
            follow();
        }
        else
        {
            sr.enabled = false;
        }
    }

    void follow()
    {
        Vector2 startPos = transform.position;
        Vector2 endPos = electricBeamAbility.Target.transform.position;
        Vector2 dir = endPos - startPos;
        transform.up = dir;
        sr.size = new Vector2(sr.size.x, dir.magnitude);
    }

    void processActivated(bool active)
    {
        this.enabled = active;
        sr.enabled = false;
        //TODO: Get back
        //Managers.Effect.showLightningStatic(electricBeamAbility.gameObject, active);
    }

    void updateStaticEffect(GameObject oldGO, GameObject newGO)
    {
        if (oldGO)
        {
            //TODO: Get back
            //Managers.Effect.showLightningStatic(oldGO, false);
        }
        if (newGO)
        {
            //TODO: Get back
            //Managers.Effect.showLightningStatic(newGO);
            sr.enabled = true;
        }
        else
        {
            sr.enabled = false;
        }
    }
}
