using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour {

    //Effects
    public GameObject collisionEffectPrefab;//the object that holds the special effect for collision
    //Supporting Lists
    private List<ParticleSystem> collisionEffectList = new List<ParticleSystem>();

    private static EffectManager instance;

	// Use this for initialization
	void Start () {
		if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
	}
	
	public static void collisionEffect(Vector2 position)
    {
        ParticleSystem chosenPS = null;
        //Find existing particle system
        foreach (ParticleSystem ps in instance.collisionEffectList)
        {
            if (!ps.isPlaying)
            {
                chosenPS = ps;
            }
        }
        //Else make a new one
        if (chosenPS == null)
        {
            GameObject ce = GameObject.Instantiate(instance.collisionEffectPrefab);
            ParticleSystem ceps = ce.GetComponent<ParticleSystem>();
            instance.collisionEffectList.Add(ceps);
            chosenPS = ceps;
        }
        chosenPS.gameObject.transform.position = position;
        chosenPS.Play();
    }
}
