using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatistics : MonoBehaviour, ISetting
{

    private Dictionary<Stat, int> stats = new Dictionary<Stat, int>();

    public string ID => "GameStatistics";
    public SettingScope Scope => SettingScope.SAVE_FILE;

    public SettingObject Setting
    {
        get =>
            new SettingObject(ID)
            .addDictionary("stats", stats);
        set
        {
            stats = value.Dictionary<Stat, int>("stats");
            printStats(false);
        }
    }

    public void addOne(Stat stat)
    {
        if (!stats.ContainsKey(stat))
        {
            stats[stat] = 0;
        }
        stats[stat]++;
    }

    public int get(Stat stat)
    {
        if (!stats.ContainsKey(stat))
        {
            stats[stat] = 0;
        }
        return stats[stat];
    }

    public void printStats(bool all)
    {
        Debug.Log("=== Printing stats ===");
        foreach (KeyValuePair<Stat, int> stat in stats)
        {
            if (all || stat.Value > 0)
            {
                Debug.Log("Stat: " + stat.Key + " = " + stat.Value);
            }
        }
    }
}
