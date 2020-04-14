using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores data about a Setting
/// </summary>
public class SettingObject
{
    public string id;
    public Dictionary<string, System.Object> data = new Dictionary<string, System.Object>();

    public SettingObject() { }

    /// <summary>
    /// Constructs a SettingObject with the given pieces of data
    /// Enter data in pairs: key,object,... 
    /// Example: "cracked",true,"name","CrackedGround"
    /// </summary>
    /// <param name="pairs"></param>
    public SettingObject(string id, params System.Object[] pairs)
    {
        this.id = id;
        if (pairs.Length % 2 != 0)
        {
            throw new UnityException("Pairs has an odd amount of parameters! pairs.Length: " + pairs.Length);
        }
        for (int i = 0; i < pairs.Length; i += 2)
        {
            data.Add((string)pairs[i], pairs[i + 1]);
        }
    }
}
