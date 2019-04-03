using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatistics : SavableMonoBehaviour
{

    private Dictionary<string, int> stats = new Dictionary<string, int>() {
        { "Tap", 0},//how many times the player has tapped
        { "Hold", 0},//how many times the player has done the hold gesture
        { "Drag", 0},//how many times the player has done the drag gesture

        { "Teleport", 0},//how many times the player has teleported
        { "ForceCharge", 0},//how many times the player has used the force charge ability
        { "WallClimb", 0},//how many times the player has teleported off a wall
        { "ElectricField", 0},//how many times the player has used the shield bubble ability

        { "Death", 0},//how many times the player has died
        { "Rewind", 0}//how many times the player used the rewind ability
    };

    private static GameStatistics instance;

    // Use this for initialization
    void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance);
        }
        instance = this;
    }

    public override SavableObject getSavableObject()
    {
        List<object> statParams = new List<object>();
        foreach (KeyValuePair<string, int> stat in stats)
        {
            statParams.Add(stat.Key);
            statParams.Add(stat.Value);
        }
        return new SavableObject(this, statParams.ToArray());
    }
    public override void acceptSavableObject(SavableObject savObj)
    {
        List<string> statNames = new List<string>(stats.Keys);
        foreach (string statName in statNames)
        {
            stats[statName] = Mathf.Max(stats[statName], (int)savObj.data[statName]);
        }
        printStats();
    }

    public static void addOne(string counterName)
    {
        checkStatName(counterName);
        instance.stats[counterName]++;
    }

    public static int get(string counterName)
    {
        checkStatName(counterName);
        return instance.stats[counterName];
    }

    /// <summary>
    /// Throws an error if the given stat name isn't being tracked
    /// </summary>
    /// <param name="statName"></param>
    private static void checkStatName(string statName)
    {
        if (!instance.stats.ContainsKey(statName))
        {
            throw new System.ArgumentException(
                "GameStatistics is not tracking that stat (" + statName + ")! "
                + "Check to make sure you spelled it correctly."
                );
        }
    }
    private void printStats()
    {
        foreach (KeyValuePair<string, int> stat in stats)
        {
            Debug.Log("Stat: " + stat.Key + " = " + stat.Value);
        }
    }
}
