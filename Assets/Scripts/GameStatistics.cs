using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatistics : SavableMonoBehaviour
{

    private Dictionary<string, int> stats = new Dictionary<string, int>() {
        { "tapCount", 0 },//how many times the player has tapped
        { "holdCount", 0},//how many times the player has done the hold gesture
        { "dragCount" , 0},//how many times the player has done the drag gesture

        { "teleportCount" , 0},//how many times the player has teleported
        { "forceWaveCount" , 0},//how many times the player has used the force wave ability
        { "teleportCountWallJump" , 0},//how many times the player has teleported off a wall
        { "shieldBubbleCount" , 0},//how many times the player has used the shield bubble ability

        { "deathCount" , 0},//how many times the player has died
        { "rewindCount" , 0}//how many times the player used the rewind ability
    };

    private static GameStatistics instance;

    // Use this for initialization
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this, "deathCount", stats["deathCount"]);
    }
    public override void acceptSavableObject(SavableObject savObj)
    {
        stats["deathCount"] = Mathf.Max(stats["deathCount"],(int)savObj.data["deathCount"]);
    }

    public static void incrementCounter(string counterName)
    {
        instance.stats[counterName]++;
    }
    public static int counter(string counterName)
    {
        return instance.stats[counterName];
    }
}
