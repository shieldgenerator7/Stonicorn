using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : SavableMonoBehaviour
{
    public float moveSpeed = 3;
    public float moveDuration = 2;
    public float teleportRange = 15;

    private Rigidbody2D rb2d;

    private float moveStartTime = -1;
    private Vector2 moveDir;

    public override void init()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    private bool piloting = false;
    public bool Piloting
    {
        get => piloting;
        set
        {
            piloting = value;
            Managers.Player.Teleport.onTeleport -= processTeleport;
            if (piloting)
            {
                Managers.Player.Teleport.onTeleport += processTeleport;
            }
        }
    }

    private void FixedUpdate()
    {
        if (moveStartTime >= 0)
        {
            if (Managers.Time.Time > moveStartTime + moveDuration)
            {
                rb2d.AddForce(moveDir * rb2d.mass * moveSpeed);
            }
            else
            {
                moveStartTime = -1;
            }
        }
    }


    private void processTeleport(Vector2 oldPos, Vector2 newPos)
    {
        moveStartTime = Managers.Time.Time;
    }
    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "piloting", piloting,
            "moveStartTime", moveStartTime,
            "moveDir", moveDir
            );
        set
        {
            piloting = value.Bool("piloting");
            Piloting = piloting;
            moveStartTime = value.Float("moveStartTime");
            moveDir = value.Vector2("moveDir");
        }
    }

}
