using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedRunTimer : MemoryMonoBehaviour
{
    public Text timeDisplay;

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
            timeDisplay.gameObject.SetActive(active);
        }
    }

    public float startTime = 0;
    public float endTime = 0;
    public float clockTime = 0;
    public float ClockTime
    {
        get { return clockTime; }
        set
        {
            clockTime = value;
            timeDisplay.text = "" + clockTime;
        }
    }

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
        if (endTime == 0)
        {
            ClockTime = Time.time - startTime;
        }
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
            ClockTime = endTime - startTime;
            if (!speedRunningEnabled)
            {
                speedRunningEnabled = true;
                GameManager.saveMemory(this);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Active = false;
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
