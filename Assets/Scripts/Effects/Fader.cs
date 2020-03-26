using UnityEngine;
using System.Collections;
using UnityEngine.U2D;
using UnityEngine.Experimental.U2D;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{

    public float startfade = 1.0f;
    public float endfade = 0.0f;
    public float duration = 1;
    public float delayTime = 0f;
    public bool destroyColliders = true;
    public bool destroyObjectOnFinish = true;
    public bool destroyScriptOnFinish = true;
    public bool isEffectOnly = true;//the object this fader is attached to is only a special effect and not a time-bound object
    public bool ignorePause = false;

    private float CurrentTime
    {
        get => (ignorePause) ? Time.unscaledTime : Time.time;
    }

    private ArrayList srs;
    private float startTime;

    // Use this for initialization
    void Start()
    {
        startTime = CurrentTime + delayTime;
        if (duration <= 0)
        {
            duration = Mathf.Abs(startfade - endfade);
        }
        srs = new ArrayList();
        srs.Add(GetComponent<SpriteRenderer>());
        srs.Add(GetComponent<SpriteShapeRenderer>());
        srs.Add(GetComponent<CanvasRenderer>());
        srs.AddRange(GetComponentsInChildren<SpriteRenderer>());
        srs.AddRange(GetComponentsInChildren<SpriteShapeRenderer>());
        srs.AddRange(GetComponentsInChildren<Image>());
        if (destroyColliders)
        {
            foreach (Collider2D bc in GetComponentsInChildren<Collider2D>())
            {
                Destroy(bc);
            }
        }
        //foreach (Renderer r in GetComponentsInChildren<Renderer>())
        //{
        //    r.sortingOrder = -1;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime <= CurrentTime)
        {
            float t = Mathf.Min(duration,(CurrentTime - startTime) / duration);//2016-03-17: copied from an answer by treasgu (http://answers.unity3d.com/questions/654836/unity2d-sprite-fade-in-and-out.html)
            foreach (Object o in srs)
            {
                if (!o)
                {
                    continue;//skip null values
                }
                if (o is SpriteRenderer)
                {
                    SpriteRenderer sr = (SpriteRenderer)o;
                    Color prevColor = sr.color;
                    sr.color = new Color(prevColor.r, prevColor.g, prevColor.b, Mathf.SmoothStep(startfade, endfade, t));
                    checkDestroy(sr.color.a);
                }
                if (o is SpriteShapeRenderer)
                {//2019-01-12: copied from section for SpriteRenderer
                    SpriteShapeRenderer sr = (SpriteShapeRenderer)o;
                    Color color = sr.color;
                    color.a = Mathf.SmoothStep(startfade, endfade, t);
                    sr.color = color;
                    checkDestroy(sr.color.a);
                }
                if (o is CanvasRenderer)
                {
                    CanvasRenderer sr = (CanvasRenderer)o;
                    float newAlpha = Mathf.SmoothStep(startfade, endfade, t);
                    sr.SetAlpha(newAlpha);
                    checkDestroy(newAlpha);
                }
                if (o is Image)
                {
                    Image sr = (Image)o;
                    float newAlpha = Mathf.SmoothStep(startfade, endfade, t);
                    Color c = sr.color;
                    c.a = newAlpha;
                    sr.color = c;
                    checkDestroy(newAlpha);
                }
            }
        }
    }

    void checkDestroy(float currentFade)
    {
        if (currentFade == endfade)
        {
            if(onFadeFinished != null)
            {
                onFadeFinished();
            }
            if (destroyObjectOnFinish)
            {
                if (isEffectOnly)
                {
                    Destroy(gameObject);
                }
                else
                {
                    GameManager.destroyObject(gameObject);
                }
            }
            else if (destroyScriptOnFinish)
            {
                Destroy(this);
            }
        }
    }

    public delegate void OnFadeFinished();
    public OnFadeFinished onFadeFinished;

}
