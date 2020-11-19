using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabController : Hazard
{
    public Color shellColor = Color.white;
    public Color eyeColor = Color.white;
    public List<SpriteRenderer> eyeSRs;

    public override bool Hazardous => false;

    // Start is called before the first frame update
    void Start()
    {
        //Shell colors
        new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>())
            .ForEach(sr => sr.color = shellColor);
        //Eye colors
        eyeSRs
            .ForEach(sr => sr.color = eyeColor);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
