using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabController : Hazard
{
    public Color shellColor = Color.white;

    public override bool Hazardous => false;

    // Start is called before the first frame update
    void Start()
    {
        new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>())
            .ForEach(sr => sr.color = shellColor);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
