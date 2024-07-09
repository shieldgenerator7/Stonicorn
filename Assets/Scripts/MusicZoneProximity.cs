using UnityEngine;

public class MusicZoneProximity  : MusicZone
{
    public Transform thingThatMoves;
    public Transform thingToBeCloseTo;
    public float threshold = 100;

    protected override bool conditionsMet(Vector2 pos)
    {
        return Vector2.Distance(pos, thingToBeCloseTo.position) <= threshold;
    }

    private void Update()
    {
        checkZone(thingThatMoves.position);
    }
}
