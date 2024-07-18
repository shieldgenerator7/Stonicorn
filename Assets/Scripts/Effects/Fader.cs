using UnityEngine;
using System.Collections;
using UnityEngine.U2D;

using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

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
    [Tooltip("True: destroys all colliders when enabled")]
    public bool destroyColliders = true;
    public enum FinishAction
    {
        DESTROY_GAMEOBJECT,
        DESTROY_SCRIPT,
        DISABLE_GAMEOBJECT,
        DISABLE_SCRIPT
    }
    public FinishAction finishAction = FinishAction.DESTROY_GAMEOBJECT;
    private bool isEffectOnly = true;
    [Tooltip("True to use unscaled time, false to use scaled time")]
    public bool ignorePause = true;

    private float CurrentTime
    {
        get => (ignorePause) ? Time.unscaledTime : Time.time;
    }

    private List<Component> srs;
    private float startTime;

    // Use this for initialization
    void OnEnable()
    {
        startTime = CurrentTime + delayTime;
        if (duration <= 0)
        {
            duration = Mathf.Abs(startfade - endfade);
        }
        srs = new List<Component>();
        srs.Add(GetComponent<SpriteRenderer>());
        srs.Add(GetComponent<SpriteShapeRenderer>());
        srs.Add(GetComponent<CanvasRenderer>());
        srs.AddRange(GetComponentsInChildren<SpriteRenderer>());
        srs.AddRange(GetComponentsInChildren<SpriteShapeRenderer>());
        srs.AddRange(GetComponentsInChildren<Image>());
        srs.Add(GetComponent<SpriteMask>());
        srs.RemoveAll(sr => sr == null);
        if (destroyColliders)
        {
            foreach (Collider2D bc in GetComponentsInChildren<Collider2D>().ToList())
            {
                Destroy(bc);
            }
        }
        //It's an effect if there are no savable components on the game object
        isEffectOnly = !gameObject.isSavable();
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime <= CurrentTime)
        {
            float t = Mathf.Min(duration, (CurrentTime - startTime) / duration);//2016-03-17: copied from an answer by treasgu (http://answers.unity3d.com/questions/654836/unity2d-sprite-fade-in-and-out.html)
            float alpha = Mathf.SmoothStep(startfade, endfade, t);
            srs.RemoveAll(o => o == null);
            foreach (Component o in srs)
            {
                if (o is SpriteRenderer)
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
                else if (o is SpriteMask)
                {
                    SpriteMask mask = (SpriteMask)o;
                    mask.enabled = alpha > 0;
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
            switch (finishAction)
            {
                case FinishAction.DESTROY_GAMEOBJECT:
                    if (isEffectOnly)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        Managers.Object.destroyObject(gameObject);
                    }
                    break;
                case FinishAction.DESTROY_SCRIPT:
                    Destroy(this);
                    break;
                case FinishAction.DISABLE_GAMEOBJECT:
                    gameObject.SetActive(false);
                    break;
                case FinishAction.DISABLE_SCRIPT:
                    enabled = false;
                    break;
                default:
                    throw new System.Exception("Invalid option!: " + finishAction);
            }
        }
    }

    public delegate void OnFadeFinished();
    public event OnFadeFinished onFadeFinished;

}
