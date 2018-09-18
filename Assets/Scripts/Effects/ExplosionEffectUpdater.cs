using UnityEngine;
using System.Collections;

public class ExplosionEffectUpdater : MonoBehaviour {//2016-04-18: copied from TeleportStarUpdater

    //2016-03-03 copied from TeleportStreakUpdater
    public float maxTimeShown = 50;
    public Vector2 start;
    public float startSize = 0.1f;
    public float finalSize;//how big it should be when it finishes
    public float waitTime = 0;

    private float distance;
    private Vector3 newV;
    private float startTime;
    private float ratio;
    private SpriteRenderer sr;
    private float baseWidth = 5;//2017-01-30: if the size of the sprite asset changes, this value needs updated
    private float baseHeight = 5;

    private float timeShown = 0;
    private bool turnedOn = false;

    // Use this for initialization
    void Start()
    {
    }
    public void init() { 
        sr = GetComponent<SpriteRenderer>();
        Vector3 bsize = sr.bounds.size;
        baseWidth = bsize.x;
        baseHeight = bsize.y;
        setSize(startSize);
    }

    // Update is called once per frame
    void Update()
    {
        if (turnedOn)
        {
            if (waitTime > 0)
            {
                sr.enabled = false;
                waitTime--;
                return;
            }
            else if (waitTime == 0)
            {
                sr.enabled = true;
                waitTime--;
            }
            timeShown = Time.time - startTime;
            ratio = timeShown / maxTimeShown;
            Color prevColor = sr.color;
            sr.color = new Color(prevColor.r, prevColor.g, prevColor.b, -ratio*ratio*0.7f+1);
            setSize((ratio*(finalSize-startSize))+startSize);
            if (timeShown >= maxTimeShown)
            {
                Destroy(gameObject);
            }
        }
    }

    public void position()
    {
        //Set the position
        transform.position = new Vector3(start.x, start.y, 0);
    }

    public void setSize(float newSize)
    {//2016-04-18: copied from TeleportRangeIndicatorUpdater.setSize(.)
        if (baseWidth > 0 && baseHeight > 0)
        {
            Vector3 newV = new Vector3(newSize / baseWidth, newSize / baseHeight, 0);
            transform.localScale = newV;
        }
    }

    public void turnOn(bool tO)
    {
        turnedOn = tO;
        gameObject.SetActive(tO);
        startTime = Time.time;
    }
}
