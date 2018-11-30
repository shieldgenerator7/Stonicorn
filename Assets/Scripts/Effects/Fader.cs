using UnityEngine;
using System.Collections;
using UnityEngine.U2D;
using UnityEngine.Experimental.U2D;

public class Fader : MonoBehaviour
{

    public float startfade = 1.0f;
    public float endfade = 0.0f;
    public float fadetime = 30;
    public float delayTime = 0f;
    public bool destroyColliders = true;

    private ArrayList srs;
    private float startTime;
    private float duration;

    // Use this for initialization
    void Start()
    {
        startTime = Time.time + delayTime;
        duration = Mathf.Abs(startfade - endfade);
        srs = new ArrayList();
        srs.Add(GetComponent<SpriteRenderer>());
        srs.Add(GetComponent<Ferr2DT_PathTerrain>());
        srs.Add(GetComponent<SpriteShapeRenderer>());
        srs.Add(GetComponent<CanvasRenderer>());
        srs.AddRange(GetComponentsInChildren<SpriteRenderer>());
        srs.AddRange(GetComponentsInChildren<Ferr2DT_PathTerrain>());
        srs.AddRange(GetComponentsInChildren<SpriteShapeRenderer>());
        if (destroyColliders)
        {
            foreach (Collider2D bc in GetComponentsInChildren<Collider2D>())
            {
                Destroy(bc);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime <= Time.time)
        {
            float t = (Time.time - startTime) / duration;//2016-03-17: copied from an answer by treasgu (http://answers.unity3d.com/questions/654836/unity2d-sprite-fade-in-and-out.html)
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
                    if (sr.color.a == endfade)
                    {
                        GameManager.destroyObject(gameObject);
                    }
                }
                if (o is Ferr2DT_PathTerrain)
                {
                    Ferr2DT_PathTerrain sr = (Ferr2DT_PathTerrain)o;
                    Color prevColor = sr.vertexColor;
                    sr.vertexColor = new Color(prevColor.r, prevColor.g, prevColor.b, Mathf.SmoothStep(startfade, endfade, t));
                    if (sr.vertexColor.a == endfade)
                    {
                        GameManager.destroyObject(gameObject);
                    }
                    Transform tf = sr.gameObject.transform;
                    float variance = 0.075f;
                    tf.position = tf.position + Utility.PerpendicularRight(tf.up).normalized * Random.Range(-variance, variance);
                }
                if (o is SpriteShapeRenderer)
                {//2018-11-30: copied from above section for Ferr2DT_PathTerrain
                    if (Mathf.SmoothStep(startfade, endfade, t) == endfade)
                    {
                        GameManager.destroyObject(gameObject);
                    }
                    SpriteShapeRenderer sr = (SpriteShapeRenderer)o;
                    Transform tf = sr.gameObject.transform;
                    float variance = 0.075f;
                    tf.position = tf.position + Utility.PerpendicularRight(tf.up).normalized * Random.Range(-variance, variance);
                }
                if (o is CanvasRenderer)
                {
                    CanvasRenderer sr = (CanvasRenderer)o;
                    float newAlpha = Mathf.SmoothStep(startfade, endfade, t);
                    sr.SetAlpha(newAlpha);
                    if (newAlpha == endfade)
                    {
                        GameManager.destroyObject(gameObject);
                    }
                }
            }
        }
    }
}
