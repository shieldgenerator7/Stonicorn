using UnityEngine;
using System.Collections;
using UnityEngine.U2D;

using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    [Range(0, 1)]
    public float startfade = 1.0f;
    [Range(0, 1)]
    public float endfade = 0.0f;
    [Range(0, 10)]
    public float duration = 1;
    [Range(0, 10)]
    public float delayTime = 0f;
    [Tooltip("True: destroys all colliders on Start()")]
    public bool destroyColliders = true;
    public bool destroyObjectOnFinish = true;
    public bool destroyScriptOnFinish = true;
    [Tooltip("True: the object this Fader is attached to " +
        "is only a special effect and not a time-bound object")]
    public bool isEffectOnly = true;
    public bool ignorePause = true;

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
            float t = Mathf.Min(duration, (CurrentTime - startTime) / duration);//2016-03-17: copied from an answer by treasgu (http://answers.unity3d.com/questions/654836/unity2d-sprite-fade-in-and-out.html)
            float alpha = Mathf.SmoothStep(startfade, endfade, t);
            foreach (Object o in srs)
            {
                if (!o)
                {
                    continue;//skip null values
                }
                else if (o is SpriteRenderer)
                {
                    SpriteRenderer sr = (SpriteRenderer)o;
                    sr.color = sr.color.adjustAlpha(alpha);
                }
                else if (o is SpriteShapeRenderer)
                {//2019-01-12: copied from section for SpriteRenderer
                    SpriteShapeRenderer ssr = (SpriteShapeRenderer)o;
                    ssr.color = ssr.color.adjustAlpha(alpha);
                }
                else if (o is CanvasRenderer)
                {
                    CanvasRenderer cr = (CanvasRenderer)o;
                    cr.SetAlpha(alpha);
                }
                else if (o is Image)
                {
                    Image img = (Image)o;
                    img.color = img.color.adjustAlpha(alpha);
                }
            }
            checkDestroy(alpha);
        }
    }

    void checkDestroy(float currentFade)
    {
        if (currentFade == endfade)
        {
            onFadeFinished?.Invoke();
            if (destroyObjectOnFinish)
            {
                if (isEffectOnly)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Managers.Object.destroyObject(gameObject);
                }
            }
            else if (destroyScriptOnFinish)
            {
                Destroy(this);
            }
        }
    }

    public delegate void OnFadeFinished();
    public event OnFadeFinished onFadeFinished;

}
