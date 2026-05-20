using UnityEngine;


//2026-05-19: copied from DistanceYTimer
public class TeleportCountTimer : Timer
{
    public Transform timerObject;

    [AutoInitialize(Container = "timerObject"), SerializeField, HideInInspector]
    private TeleportAbility teleportAbility;

    [SerializeField, HideInInspector]
    private int teleportsUsed = 0;
    public int maxTeleportCount = 50;

    public override float Duration
    {
        get => maxTeleportCount;
        set => maxTeleportCount = (int)value;
    }

    public override bool Active
    {
        get => teleportsUsed < maxTeleportCount;
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

    public override float CurrentTime => Duration - TimeLeft;

    public override float TimeLeft
    {
        get => maxTeleportCount - teleportsUsed;
        set => teleportsUsed = (int)value;
    }

    public override void overrideStartTime(float startTime)
    {
        teleportAbility.onTeleport -= onTeleport;
        teleportAbility.onTeleport += onTeleport;

        teleportsUsed = 0;
    }

    public override void setTimer(float seconds = 0)
    {
        teleportsUsed = (int)seconds;
    }

    void onTeleport(Vector2 oldpos, Vector2 newpos)
    {
        teleportsUsed++;
        if (teleportsUsed >= maxTeleportCount)
        {
            teleportsUsed = maxTeleportCount;
            callOnTimeFinished();
            Active = false;
        }
    }

}
