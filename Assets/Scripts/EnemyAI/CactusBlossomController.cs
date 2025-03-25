using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CactusBlossomController : SavableMonoBehaviour
{
    [Tooltip("How far to the left to open to. 0 is pointing to its left")]
    public float openLeft = 0;
    [Tooltip("How far to the right to open to. 0 is pointing to its left")]
    public float openRight = 180;
    [Tooltip("How much to nudge the middle petals by")]
    public float middleOffset = 0;
    [Range(0f, 1f)]
    public float openPercent = 1f;

    [Tooltip("How long it takes to close while closing")]
    public float closingDuration = 0.5f;
    [Tooltip("How long it stays closed")]
    public float closedDuration = 5;
    [Tooltip("How long it takes to open while opening")]
    public float openingDuration = 2;

    public List<Transform> petals;

    public float OpenPercent
    {
        get => openPercent;
        set
        {
            openPercent = Mathf.Clamp(value, 0, 1);
        }
    }
    private float closedWaitStartTime = 0;

    public enum State
    {
        OPEN,
        CLOSING,
        CLOSED,
        OPENING,
    }
    public State state = State.OPEN;

    public override void init()
    {
        state = (openPercent==1)? State.OPEN : State.CLOSED;
        placePetals(openPercent);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        init();
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.OPEN:
                break;
            case State.CLOSING:
                OpenPercent += -(1 / closingDuration) * Time.deltaTime;
                placePetals(openPercent);
                if (openPercent == 0)
                {
                    state = State.CLOSED;
                    closedWaitStartTime = Managers.Time.Time;
                }
                break;
            case State.CLOSED:
                if (Managers.Time.Time - closedWaitStartTime >= closedDuration)
                {
                    shiftToOpen();
                }
                break;
            case State.OPENING:
                OpenPercent += (1 / openingDuration) * Time.deltaTime;
                placePetals(openPercent);
                if (openPercent == 1)
                {
                    state = State.OPEN;
                }
                break;
            default:
                throw new UnityException($"Unknown state: {state}");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TeleportAbility ta = collision.GetComponent<TeleportAbility>();
        if (ta)
        {
            ta.onTeleport -= reactToTeleport;
            ta.onTeleport += reactToTeleport;
        }
        ForceLaunchAbility fla = collision.GetComponent<ForceLaunchAbility>();
        if (fla)
        {
            fla.onLaunch -= reactToLaunch;
            fla.onLaunch += reactToLaunch;
            if (fla.AffectingVelocity)
            {
                reactToLaunch();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        TeleportAbility ta = collision.GetComponent<TeleportAbility>();
        if (ta)
        {
            ta.onTeleport -= reactToTeleport;
        }
        ForceLaunchAbility fla = collision.GetComponent<ForceLaunchAbility>();
        if (fla)
        {
            fla.onLaunch -= reactToLaunch;
        }
    }

    public void reactToTeleport(Vector2 oldPos, Vector2 newPos)
    {
        shiftToOpen();
    }
    public void reactToLaunch()
    {
        shiftToClose();
    }
    void shiftToOpen()
    {
        //early exit: already open
        if (state == State.OPEN) { return; }

        //start the opening process
        state = State.OPENING;
    }
    void shiftToClose() { 
        //early exit: already closed
        if (state == State.CLOSED)        {            return;        }

        //start the closing process
        state = State.CLOSING;
    }

    public void placePetals(float openPercent)
    {
        float openDiff = openRight - openLeft;
        float openHalf = openDiff / 2;

        float sign = Mathf.Sign(openDiff);
        float closePercent = 1 - openPercent;
        float left = openLeft + (openHalf * closePercent);
        float diff = openDiff - (Mathf.Abs(openLeft - left) * 2 * sign);

        for(int i = 0; i < petals.Count; i++)
        {
            Transform petal = petals[i];
            float percent = (float)i / (float)(petals.Count - 1);
            float offset = (i > 0 && i < petals.Count - 1) ? middleOffset * openPercent : 0;
            float angle = diff * percent + left + offset;
            petal.localEulerAngles = new Vector3(0,0,angle);
        }
    }

    static short key_state = 0;
    static short key_openPercent = 1;
    static short key_closedWaitStartTime = 2;
    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
           key_state, (int)state,
           key_openPercent, openPercent,
           key_closedWaitStartTime, closedWaitStartTime
           );
        set
        {
            state = (State)value.Int(key_state);
            openPercent = value.Float(key_openPercent);
            closedWaitStartTime = value.Float(key_closedWaitStartTime);
            placePetals(openPercent);
        }
    }
}   
