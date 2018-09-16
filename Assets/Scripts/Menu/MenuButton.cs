using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour {

    private BoxCollider2D bc2d;

	// Use this for initialization
	void Start () {
        bc2d = GetComponent<BoxCollider2D>();
	}
	
    public bool tapInArea(Vector3 pos)
    {
        return bc2d.OverlapPoint(pos);
    }

    public void activate()
    {
        Debug.Log("MenuButton " + name + " pressed");
    }
}
