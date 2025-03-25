using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (GravityAccepter))]
public class BalloonController : SavableMonoBehaviour
{

    public float peakMax = 0.02f;
    public float peakMin = -0.02f;
    public float cycleLength = 1f;

    private float pingpong;
    private float timeElapsed = 0;

    [SerializeField]
    [AutoInitialize]
    private GravityAccepter ga;

    public override void init()
    {
    }

    private void FixedUpdate()
    {
        float peakDiff = peakMax - peakMin;
        timeElapsed += Time.fixedDeltaTime;
        pingpong = Mathf.PingPong(timeElapsed * peakDiff / (2 * cycleLength), peakDiff);
        pingpong += peakMin;
        ga.gravityScale = pingpong;
    }

    static byte key_timeElapsed = 0;
    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            key_timeElapsed, timeElapsed
            );
        set
        {
            timeElapsed = value.Float(key_timeElapsed);
        }
    }
}
