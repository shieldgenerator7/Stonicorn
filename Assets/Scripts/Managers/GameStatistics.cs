using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatistics : MonoBehaviour, ISetting
{

    private Dictionary<string, int> stats = new Dictionary<string, int>() {
        { "Tap", 0},//how many times the player has tapped
        { "Hold", 0},//how many times the player has done the hold gesture
        { "Drag", 0},//how many times the player has done the drag gesture
        { "Pinch", 0},//how many times the player has done the pinch gesture (including mouse wheel)

        { "Teleport", 0},//how many times the player has teleported
        { "ForceChargeBoost", 0},//how many times the player has boosted with the force charge ability
        { "ForceChargeWake", 0},//how many wakes the player has made
        { "ForceChargeBlast", 0},//how many times the player has blasted with the force charge ability
        { "Swap", 0},//how many times the player has swapped during a teleport
        { "SwapObject", 0},//how many objects the player has swapped with
        { "WallClimb", 0},//how many times the player has teleported off a wall
        { "WallClimbSticky", 0},//how many sticky pads the player has made
        { "ElectricField", 0},//how many times the player has activated the electric field ability
        { "ElectricFieldField", 0},//how many electric fields the player has created
        { "AirSlice", 0},//how many times the player has teleported in the air with the air slice ability
        { "AirSliceObject", 0},//how many objects the player has sliced
        { "LongTeleport", 0},//how many times the player has teleported with an extended range

        { "Damaged", 0},//how many times the player has been damaged
        { "Rewind", 0},//how many times the rewind ability has been activated
        { "RewindPlayer", 0}//how many times the player used the rewind ability
    };

    public string ID => "GameStatistics";
    public SettingScope Scope => SettingScope.SAVE_FILE;

    public SettingObject Setting
    {
        get =>
            new SettingObject(ID)
            .addDictionary("stats", stats);
        set
        {
            stats = value.Dictionary<string, int>("stats");
            printStats(false);
        }
    }

    public void addOne(string counterName)
    {
        checkStatName(counterName);
        stats[counterName]++;
    }

    public int get(string counterName)
    {
        checkStatName(counterName);
        return stats[counterName];
    }

    /// <summary>
    /// Throws an error if the given stat name isn't being tracked
    /// </summary>
    /// <param name="statName"></param>
    private void checkStatName(string statName)
    {
        if (!stats.ContainsKey(statName))
        {
            throw new System.ArgumentException(
                "GameStatistics is not tracking that stat (" + statName + ")! "
                + "Check to make sure you spelled it correctly."
                );
        }
    }
    public void printStats(bool all)
    {
        Debug.Log("=== Printing stats ===");
        foreach (KeyValuePair<string, int> stat in stats)
        {
            if (all || stat.Value > 0)
            {
                Debug.Log("Stat: " + stat.Key + " = " + stat.Value);
            }
        }
    }
}
