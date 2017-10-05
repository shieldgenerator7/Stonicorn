using UnityEngine;
using System.Collections;

public class WeightSwitchActivator : MonoBehaviour {

    public bool pressed = false;
    public GameObject psgoSparks;
    private ParticleSystem psSparks;
    public ParticleSystem psExhaust;

    // Use this for initialization
    void Start () {
        psSparks = psgoSparks.GetComponent<ParticleSystem>();
        if (psSparks != null)
        {
            psSparks.Pause();
            psSparks.Clear();
        }
    }
	
	void FixedUpdate () {
        pressed = false;
        //Determine if anything is in the weight switch's trigger area
        BoxCollider2D bc2d = GetComponentInChildren<BoxCollider2D>();
        if (bc2d != null)
        {
            Collider2D[] hitColliders = Physics2D.OverlapAreaAll(bc2d.bounds.min, bc2d.bounds.max);
            for (int i = 0; i < hitColliders.Length; i++)
            {
                GameObject hc = hitColliders[i].gameObject;
                if (!hc.Equals(gameObject) && !hc.Equals(bc2d.transform.gameObject))
                {
                    if (hc.GetComponent<Rigidbody2D>() != null)
                    {
                        pressed = true;
                        break;
                    }
                }
            }
        }
    }

    void Update()
    {
        if (psSparks != null)
        {
            if(pressed){
                psSparks.Play();
                psExhaust.Play();
            }
            else
            {
                psSparks.Pause();
                psSparks.Clear();
                psExhaust.Pause();
                psExhaust.Clear();
            }
        }
    }
    
}
