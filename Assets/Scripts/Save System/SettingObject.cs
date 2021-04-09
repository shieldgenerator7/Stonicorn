using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public SettingObject addList<T>(string key, List<T> list)
    {
        int index = 0;
        data.Add(key + "_count", list.Count);
        list.ForEach(item =>
        {
            data.Add(key + index, item);
            index++;
        });
        return this;
    }
    public List<T> List<T>(string key)
    {
        List<T> list = new List<T>();
        int count = (int)data[key + "_count"];
        for (int i = 0; i < count; i++)
        {
            list.Add(
                (T)data[key + i]
                );
        }
        return list;
    }

    //2021-01-19: copied from SavableObject
    public SettingObject addDictionary<K, V>(string key, Dictionary<K, V> dict)
    {
        int index = 0;
        data.Add(key + "_count", dict.Count);
        dict.ToList().ForEach(entry =>
        {
            data.Add(key + index + "K", entry.Key);
            data.Add(key + index + "V", entry.Value);
            index++;
        });
        return this;
    }
    public Dictionary<K, V> Dictionary<K, V>(string key)
    {
        Dictionary<K, V> dict = new Dictionary<K, V>();
        int count = (int)data[key + "_count"];
        for (int i = 0; i < count; i++)
        {
            dict.Add(
                (K)data[key + i + "K"],
                (V)data[key + i + "V"]
                );
        }
        return dict;
    }

    public static implicit operator bool(SettingObject so)
        => so != null;
}
