using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CactusBlossomController : MonoBehaviour
{
    [Tooltip("How far to the left to open to. 0 is pointing to its left")]
    public float openLeft = 0;
    [Tooltip("How far to the right to open to. 0 is pointing to its left")]
    public float openRight = 180;
    [Tooltip("How much to nudge the middle petals by")]
    public float middleOffset = 0;
    [Range(0f, 1f)]
    public float _openPercent = 1f;

    [Tooltip("How long it takes to close while closing")]
    public float closingDuration = 0.5f;
    [Tooltip("How long it stays closed")]
    public float closedDuration = 5;
    [Tooltip("How long it takes to open while opening")]
    public float openingDuration = 2;

    public List<Transform> petals;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        placePetals(_openPercent);
    }

    // Update is called once per frame
    void Update()
    {
        //placePetals(_openPercent);
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
        Timer closingTimer = Timer.startTimer(closingDuration, () => {
            Timer closedTimer = Timer.startTimer(closedDuration, () => {
                Timer openingTimer = Timer.startTimer(openingDuration, () => { });
                openingTimer.onTimeLeftChanged += (timeLeft, duration) =>
                {
                    placePetals(1 - (timeLeft / duration));
                };
            });
        });
        closingTimer.onTimeLeftChanged += (timeLeft, duration) =>
        {
            placePetals(timeLeft / duration);
        };
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
}
