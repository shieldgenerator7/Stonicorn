using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceYTimer : Timer
{
    [Header("Distance Timer Settings")]
    public Transform timerObject;
    public Transform targetObject;

    private Rigidbody2D rb2dTimer;

    private Vector2 origPos;
    public override float Duration
    {
        get => OrigDistance / rb2dTimer.linearVelocity.magnitude;
        set { }
    }
    public override bool Active
    {
        get => Distance > 0;
        set
        {
            if (!value)
            {
                if (destroyOnFinish)
                {
                    Destroy(this);
                }
            }
        }
    }
    public override float TimeLeft
    {
        get => Distance / rb2dTimer.linearVelocity.magnitude;
        set { }
    }

    public override float CurrentTime => Duration - TimeLeft;

    private float Distance => Mathf.Abs(timerObject.position.y - targetObject.position.y);
    private float OrigDistance => Mathf.Abs(origPos.y - targetObject.position.y);

    public override void overrideStartTime(float startTime)
    {
        //do nothing
    }

    public override void setTimer(float seconds = 0)
    {
        //do nothing
    }

    protected override void Start()
    {
        origPos = timerObject.position;
        rb2dTimer = timerObject.GetComponent<Rigidbody2D>();
    }

    protected override void Update()
    {
        if (Active)
        {
            float timeLeft = TimeLeft;
            callOnTimeLeftChanged(timeLeft, Duration);
            if (timeLeft <= 0)
            {
                callOnTimeFinished();
                Active = false;
            }
        }
    }
}
