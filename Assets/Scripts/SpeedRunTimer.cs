using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedRunTimer : MemoryMonoBehaviour
{
    /// <summary>
    /// True if the player has unlocked speedrunning mode as an option
    /// </summary>
    public bool speedRunningEnabled = false;

    private static bool active = false;
    /// <summary>
    /// True if the player has manually activated speedrunning mode
    /// </summary>
    public bool Active
    {
        get { return active; }
        private set
        {
            active = value;
            enabled = active;
            GetComponent<SpriteRenderer>().enabled = active;
        }
    }

    public float startTime = 0;
    public float endTime = 0;
    public float clockTime = 0;

    private void Start()
    {
        Active = active;
        if (active)
        {
            GameManager.GestureManager.tapGesture += waitForNextTap;
        }
    }

    private void Update()
    {
        clockTime = Time.time - startTime;
    }

    public void activate()
    {
        Active = true;
        GameManager.resetGame();
    }

    void waitForNextTap()
    {
        startTime = Time.time;
        endTime = 0;
        GameManager.GestureManager.tapGesture -= waitForNextTap;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == GameManager.getPlayerObject())
        {
            endTime = Time.time;
            clockTime = endTime - startTime;
            Active = false;
            if (!speedRunningEnabled)
            {
                speedRunningEnabled = true;
                GameManager.saveMemory(this);
            }
        }
    }
    public override MemoryObject getMemoryObject()
    {
        return new MemoryObject(this, speedRunningEnabled);
    }
    public override void acceptMemoryObject(MemoryObject memObj)
    {
        speedRunningEnabled = memObj.found;
    }
}
