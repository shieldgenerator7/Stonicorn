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

    private GravityAccepter ga;

    // Start is called before the first frame update
    void Start()
    {
        ga = GetComponent<GravityAccepter>();
    }

    private void FixedUpdate()
    {
        float peakDiff = peakMax - peakMin;
        timeElapsed += Time.fixedDeltaTime;
        pingpong = Mathf.PingPong(timeElapsed * peakDiff / (2 * cycleLength), peakDiff);
        pingpong += peakMin;
        ga.gravityScale = pingpong;
    }

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this,
            "timeElapsed", timeElapsed
            );
    }

    public override void acceptSavableObject(SavableObject savObj)
    {
        timeElapsed = (float)savObj.data["timeElapsed"];
    }
}
