using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedEffectController : MonoBehaviour
{
    public float duration = 1;
    public enum RepeatOption
    {
        LOOP,
        PLAY_ONCE,
        PING_PONG
    }
    public RepeatOption repeatOption = RepeatOption.LOOP;

    public List<TimedEffect> effects;

    private float startTime;
    [SerializeField]
    [Range(0, 1)]
    private float time;

    // Start is called before the first frame update
    void OnEnable()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        float currentTime = 0;
        switch (repeatOption)
        {
            case RepeatOption.LOOP:
                currentTime = (Time.time - startTime) % duration;
                break;
            case RepeatOption.PLAY_ONCE:
                currentTime = Mathf.Min(Time.time - startTime, duration);
                break;
            case RepeatOption.PING_PONG:
                float timeDiff = Time.time - startTime;
                if (Mathf.Floor(timeDiff / duration) % 2 == 0)
                {
                    currentTime = timeDiff % duration;
                }
                else
                {
                    currentTime = duration - (timeDiff % duration);
                }
                break;
        }

        time = currentTime / duration;
        effects.ForEach(fx => fx.processEffect(time));
    }
}
