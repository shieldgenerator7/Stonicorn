using UnityEngine;
using System.Collections;

public class TeleportStarUpdater : MonoBehaviour
{
    //2016-03-03 copied from TeleportStreakUpdater
    //2017-10-31 refactored

    public float duration = 2;//the duration of the effect in seconds
    public Color baseColor = new Color(1, 1, 1);

    private float startTime = 0;//the time the effect was begun
    private bool turnedOn = false;
    public bool TurnedOn
    {
        get { return turnedOn; }
        set
        {
            turnedOn = value;
            gameObject.SetActive(value);
            if (value)
            {
                startTime = Time.unscaledTime;
                transform.localScale = baseScale;
                baseColor.a = 1;
            }
        }
    }

    private SpriteRenderer sr;
    private Vector3 baseScale;
    private float shrinkRate;
    private Vector3 shrinkVector;

    // Use this for initialization
    public void init()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = baseColor;
        baseScale = transform.localScale;
        shrinkRate = duration * Time.unscaledDeltaTime;
        shrinkVector = new Vector3(shrinkRate, shrinkRate);
    }

    // Update is called once per frame
    public void updateStar()
    {
        baseColor.a -= shrinkRate;
        sr.color = baseColor;
        transform.Rotate(Vector3.forward * -10);//2016-03-03: copied from an answer by Eric5h5: http://answers.unity3d.com/questions/580001/trying-to-rotate-a-2d-sprite.html
        transform.localScale -= shrinkVector;
        if (Time.unscaledTime > startTime + duration)
        {
            TurnedOn = false;
        }
    }

    public void position(Vector2 pos)
    {
        //Set the position
        transform.position = new Vector3(pos.x, pos.y, 1);
    }
}
