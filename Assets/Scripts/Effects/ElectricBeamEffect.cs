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
        electricBeamAbility.onActivatedChanged += (active) =>
        {
            this.enabled = active;
            sr.enabled = false;
        };
        this.enabled = electricBeamAbility.Activated;
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
}
