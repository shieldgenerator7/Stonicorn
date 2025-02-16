using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBeamEffect : MonoBehaviour
{
    public ElectricBeamAbility electricBeamAbility;

    [AutoInitialize,SerializeField,HideInInspector]
    private SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        sr.enabled = false;
        electricBeamAbility.onActivatedChanged += processActivated;
        electricBeamAbility.onTargetChanged += updateStaticEffect;
        processActivated(electricBeamAbility.Activated);
        updateStaticEffect(null, electricBeamAbility.Target);
    }

    // Update is called once per frame
    void Update()
    {
        if (electricBeamAbility.Target != null)
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
        Vector2 endPos = electricBeamAbility.Target.GameObject.transform.position;
        Vector2 dir = endPos - startPos;
        transform.up = dir;
        sr.size = new Vector2(sr.size.x, dir.magnitude);
    }

    void processActivated(bool active)
    {
        this.enabled = active;
        sr.enabled = false;
        Managers.Effect.showLightningStatic(electricBeamAbility.gameObject, active);
    }

    void updateStaticEffect(IPowerable oldPowerable, IPowerable newPowerable)
    {
        if (oldPowerable != null)
        {
            Managers.Effect.showLightningStatic(oldPowerable.GameObject, false);
        }
        if (newPowerable != null)
        {
            Managers.Effect.showLightningStatic(newPowerable.GameObject);
            sr.enabled = true;
        }
        else
        {
            sr.enabled = false;
        }
    }
}
