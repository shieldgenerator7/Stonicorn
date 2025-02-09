using UnityEngine;
using System.Collections;

public class CloudDrifter : MonoBehaviour {

    public float speed = 0.002f;
    public GameObject parentCloud = null;
    public float range = 5f;
    public float variance = 0.002f;

    private float maxDiff;//only set at start up
    private float diff;
    private float baseSpeed;//test changed line
    //comment for test commit

    [AutoInitialize, SerializeField, HideInInspector]
    private SpriteRenderer sr;

	// Use this for initialization
	void Start () {
         maxDiff = Vector3.Distance(new Vector3(0f, 0f), new Vector3(range, range));
    }
	
	// Update is called once per frame
	void Update ()
    {
        //float diff = Vector3.Distance(parentCloud.transform.position, transform.position);
        float alpha = 1f - (diff / maxDiff);
        sr.color = new Color(1f, 1f, 1f, alpha);
    }

    void FixedUpdate()
    {
        if (parentCloud != null)
        {
            transform.position += new Vector3(speed, 0f);
             diff = Vector3.Distance(parentCloud.transform.position, transform.position);
            if (diff > maxDiff)
            {
                transform.position = new Vector3(parentCloud.transform.position.x - range, Random.Range(-range, range));
                speed = baseSpeed + Random.Range(-variance, variance);
            }
        }
    }

    public void init(GameObject parent)
    {
        parentCloud = parent;
        speed += Random.Range(-variance, variance);
        baseSpeed = parentCloud.GetComponent<CloudDrifter>().speed;
    }
}
