using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunController : Hazard
{
    public Vector2 targetPos = Vector2.zero;
    public float travelTimeToTarget = 360;

    private float speed;
    private Vector2 direction;

    public override void init()
    {
        direction = targetPos - (Vector2)transform.position;
        speed = direction.magnitude / travelTimeToTarget;
        GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * speed;
    }

    private void Update()
    {
        //Check if checkpoints are in working order
        //"in owrking order" == not in sun
        Managers.ActiveCheckPoints.ForEach(cp =>
        {
            bool inSun = cp.transform.position.y - 2 < transform.position.y;
            if (cp.InWorkingOrder == inSun)
            {
                cp.InWorkingOrder = !inSun;
            }
        });
        //Check if player has made contact with the sun
        if (Managers.Player.transform.position.y - 0.5 < transform.position.y)
        {
            Managers.Player.forceRewindHazard(this.DamageDealt, new Vector2(Managers.Player.transform.position.x, transform.position.y));
        }
    }
}
