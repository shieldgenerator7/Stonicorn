using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaderProximity : MonoBehaviour
{

    public GameObject proximityObject;//object to check proximity for
    public float fadeInStartRange = 5;//how far to start fading it in
    public float fullInRadius = 1;//how far out it will always be fully opaque

    private SpriteRenderer sr;

    // Use this for initialization
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector2.Distance(
            proximityObject.transform.position,
            transform.position
            );
        if (distance <= fadeInStartRange)
        {
            sr.color = sr.color.adjustAlpha(
                1 - ((distance - fullInRadius) / (fadeInStartRange - fullInRadius))
                );
        }
        else
        {
            sr.color = sr.color.adjustAlpha(0);
        }
    }
}
