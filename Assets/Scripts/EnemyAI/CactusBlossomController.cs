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
    private float closedWaitTime = 0;

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
                    closedWaitTime = 0;
                }
                break;
            case State.CLOSED:
                closedWaitTime += (1 / closedDuration) * Time.deltaTime;
                if (closedWaitTime >= closedDuration)
                {
                    state = State.OPENING;
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
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        TeleportAbility ta = collision.GetComponent<TeleportAbility>();
        if (ta)
        {
            ta.onTeleport -= reactToTeleport;
        }
    }

    public void reactToTeleport(Vector2 oldPos, Vector2 newPos)
    {
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

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
           "state", (int)state,
           "openPercent", openPercent,
            "closedWaitTime", closedWaitTime
           );
        set
        {
            state = (State)value.Int("state");
            openPercent = value.Float("openPercent");
            closedWaitTime = value.Float("closedWaitTime");
            placePetals(openPercent);
        }
    }
}   
