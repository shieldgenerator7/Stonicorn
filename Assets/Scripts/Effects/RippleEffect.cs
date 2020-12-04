using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RippleEffect : MonoBehaviour
{
    [Header("Size")]
    public float size1 = 0;
    public float size2 = 1;

    [Header("Alpha")]
    public float alpha1 = 0.5f;
    public float alpha2 = 0;

    [Header("Time")]
    public float duration = 3;
    public float offset = 0;

    private float lastStartTime = 0;

    private SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        lastStartTime = Time.time + offset;
    }

    // Update is called once per frame
    void Update()
    {
        float percent = Mathf.Clamp(
            (Managers.Time.Time - lastStartTime) / duration,
            0,
            1
            );
        //Alpha
        Color color = sr.color;
        color.a = (alpha2 - alpha1) * percent + alpha1;
        sr.color = color;
        //Size
        transform.localScale = Vector2.one * ((size2 - size1) * percent + size1);
        //Time
        if (Managers.Time.beat(duration, offset))
        {
            lastStartTime = Managers.Time.Time;
        }
    }
}
