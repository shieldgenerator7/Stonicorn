using UnityEngine;
using System.Collections;
using UnityEngine.U2D;
using UnityEngine.Experimental.U2D;

public class Fader : MonoBehaviour
{

    public float startfade = 1.0f;
    public float endfade = 0.0f;
    public float duration = 1;
    public float delayTime = 0f;
    public bool destroyColliders = true;
    public bool destroyObjectOnFinish = true;
    public bool destroyScriptOnFinish = true;

    private ArrayList srs;
    private float startTime;

    // Use this for initialization
    void Start()
    {
        startTime = Time.time + delayTime;
        if (duration <= 0)
        {
            duration = Mathf.Abs(startfade - endfade);
        }
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
        //foreach (Renderer r in GetComponentsInChildren<Renderer>())
        //{
        //    r.sortingOrder = -1;
        //}
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
                    checkDestroy(sr.color.a);
                }
                if (o is Ferr2DT_PathTerrain)
                {
                    Ferr2DT_PathTerrain sr = (Ferr2DT_PathTerrain)o;
                    Color prevColor = sr.vertexColor;
                    sr.vertexColor = new Color(prevColor.r, prevColor.g, prevColor.b, Mathf.SmoothStep(startfade, endfade, t));
                    checkDestroy(sr.vertexColor.a);
                    Transform tf = sr.gameObject.transform;
                    float variance = 0.075f;
                    tf.position = tf.position + Utility.PerpendicularRight(tf.up).normalized * Random.Range(-variance, variance);
                }
                if (o is SpriteShapeRenderer)
                {//2018-11-30: copied from above section for Ferr2DT_PathTerrain
                    checkDestroy((Mathf.SmoothStep(startfade, endfade, t)));
                    SpriteShapeRenderer sr = (SpriteShapeRenderer)o;
                    Transform tf = sr.gameObject.transform;
                    Vector2 moveDir = tf.position - GameManager.getPlayerObject().transform.position;
                    float speed = 0.075f;
                    tf.position += (Vector3)moveDir.normalized * speed;
                    float variance = 0.075f;
                    tf.position += Utility.PerpendicularRight(moveDir).normalized * Random.Range(-variance, variance);
                }
                if (o is CanvasRenderer)
                {
                    CanvasRenderer sr = (CanvasRenderer)o;
                    float newAlpha = Mathf.SmoothStep(startfade, endfade, t);
                    sr.SetAlpha(newAlpha);
                    checkDestroy(newAlpha);
                }
            }
        }
    }

    void checkDestroy(float currentFade)
    {
        if (currentFade == endfade)
        {
            if (destroyObjectOnFinish)
            {
                GameManager.destroyObject(gameObject);
            }
            else if (destroyScriptOnFinish)
            {
                Destroy(this);
            }
        }
    }
}
