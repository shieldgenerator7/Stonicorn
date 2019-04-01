using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCPrincessController : MonoBehaviour
{

    public Vector3 offsetVector;
    public float holdCooldown = 1.0f;//how long between holds (sec)

    bool hasTriggered = false;
    float startHoldTime = 0;
    float soonestNextHold = 0;

    private ForceTeleportAbility fta;
    private ElectricFieldAbility efa;
    private PlayerAbility ability;

    private float maxHoldTime;

    // Use this for initialization
    void Start()
    {
        fta = GetComponent<ForceTeleportAbility>();
        if (fta)
        {
            maxHoldTime = fta.maxCharge;
            ability = fta;
        }
        efa = GetComponent<ElectricFieldAbility>();
        if (efa)
        {
            maxHoldTime = efa.maxHoldTime;
            ability = efa;
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (hasTriggered)
        {
            if (startHoldTime == 0)
            {
                startHoldTime = Time.time;
            }
            float timeDiff = Time.time - startHoldTime;
            if (timeDiff < maxHoldTime)
            {
                ability.processHoldGesture(transform.position + offsetVector, timeDiff, false);
            }
            else
            {
                if (soonestNextHold == 0)
                {
                    soonestNextHold = Time.time + holdCooldown;
                    ability.processHoldGesture(transform.position + offsetVector, timeDiff, true);
                }
                else
                {
                    ability.dropHoldGesture();
                    if (soonestNextHold < Time.time)
                    {
                        soonestNextHold = 0;
                        startHoldTime = 0;
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.isPlayer())
        {
            hasTriggered = true;
        }
    }
    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.gameObject.isPlayer())
        {
            hasTriggered = false;
            ability.dropHoldGesture();
        }
    }
}
