using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaderProximity : MonoBehaviour {

    public GameObject proximityObject;//object to check proximity for
    public float fadeInStartRange = 5;//how far to start fading it in
    public float fullInRadius = 1;//how far out it will always be fully opaque

    private SpriteRenderer sr;

	// Use this for initialization
	void Start () {
        sr = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (((Vector2)proximityObject.transform.position - (Vector2)transform.position).sqrMagnitude <= fadeInStartRange * fadeInStartRange)
        {
            Color c = sr.color;
            c.a = 1 - (
                (((Vector2)proximityObject.transform.position - (Vector2)transform.position).magnitude-fullInRadius)
                / (fadeInStartRange-fullInRadius)
                );
            sr.color = c;
        }
        else
        {
            Color c = sr.color;
            c.a = 0;
            sr.color = c;
        }
	}
}
