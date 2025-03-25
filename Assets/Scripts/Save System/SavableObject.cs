
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// This class stores variables that need to be saved from SavableMonoBehaviours
/// </summary>
public class SavableObject
{
    [ES3Serializable]
    private Dictionary<short, System.Object> data = new Dictionary<short, System.Object>();
    /// <summary>
    /// True if it's an object that spawned during runtime
    /// </summary>
    public bool isSpawnedScript;//whether this SO's script was attached to its game object during run time
    public string scriptType;//the type of script that saved this SavableObject

    public SavableObject() { }

    /// <summary>
    /// Constructs a SavableObject with the given pieces of data
    /// Enter data in pairs: key,object,... 
    /// Example: "cracked",true,"name","CrackedGround"
    /// </summary>
    /// <param name="pairs"></param>
    public SavableObject(SavableMonoBehaviour smb, params System.Object[] pairs)
    {
        this.scriptType = smb.GetType().Name;

        more(pairs);

        if (smb.IsSpawnedScript)
        {
            isSpawnedScript = true;
        }
    }

    /// <summary>
    /// Used to add things to an existing SavableObject
    /// and return it in one line
    /// </summary>
    /// <param name="pairs"></param>
    /// <returns></returns>
    public SavableObject more(params System.Object[] pairs)
    {
        if (pairs.Length % 2 != 0)
        {
            throw new UnityException("Pairs has an odd amount of parameters! pairs.Length: " + pairs.Length);
        }
        for (int i = 0; i < pairs.Length; i += 2)
        {
#if UNITY_EDITOR
            //If it's a dictionary,
            //2020-12-31: copied from https://stackoverflow.com/a/16956978/2336212
            Type t = pairs[i + 1].GetType();
            if (t.IsGenericType
                && (t.GetGenericTypeDefinition() == typeof(List<>)
                || t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                )
            {
                throw new ArgumentException(
                    "Script " + scriptType + " is trying to store a List or Dictionary "
                    + "at key: " + pairs[i] + "; "
                    + "This is not allowed, use addList() or addDictionary() instead."
                    );
            }
#endif
            data.Add((short)pairs[i], pairs[i + 1]);
        }
        return this;
    }

    public System.Object get(short id)
        => data[id];
    public bool Bool(short id)
        => (bool)data[id];
    public int Int(short id)
        => (int)data[id];
    public float Float(short id)
        => (float)data[id];
    public short String(short id)
        => (short)data[id];
    public Vector2 Vector2(short id)
        => (Vector2)data[id];

    private static short count_offset = 100;
    private static short index_offset = 101;//meaning ~155 possible indices in an array, and only one array/dict possible per savable object
    public SavableObject addList<T>(short key, List<T> list)
    {
        int index = 0;
        data.Add((short)(key +count_offset), list.Count);
        list.ForEach(item =>
        {
            data.Add((short)(key+index_offset), item);
            index++;
        });
        return this;
    }
    public List<T> List<T>(short key)
    {
        List<T> list = new List<T>();
        int count = (int)data[(short)(key + count_offset)];
        for (int i = 0; i < count; i++)
        {
            list.Add(
                (T)data[(short)(key + index_offset)]
                );
        }
        return list;
    }

    public SavableObject addDictionary<K, V>(short key, Dictionary<K, V> dict)
    {
        int index = 0;
        data.Add((short)(key + count_offset), dict.Count);
        dict.ToList().ForEach(entry =>
        {
            data.Add((short)(key + index_offset + index *2+0), entry.Key);
            data.Add((short)(key + index_offset + index*2+1), entry.Value);
            index++;
        });
        return this;
    }
    public Dictionary<K, V> Dictionary<K, V>(short key)
    {
        Dictionary<K, V> dict = new Dictionary<K, V>();
        int count = (int)data[(short)(key + count_offset)];
        for (int i = 0; i < count; i++)
        {
            dict.Add(
                (K)data[(short)(key + index_offset + i * 2 + 0)],
                (V)data[(short)(key + index_offset + i * 2 + 1)]
                );
        }
        return dict;
    }

    public System.Type ScriptType
    {
        get
        {
            Assembly asm = typeof(SavableObject).Assembly;
            return asm.GetType(scriptType);
        }
    }

    ///<summary>
    ///Adds this SavableObject's SavableMonobehaviour to the given GameObject
    ///</summary>
    ///<param name="go">The GameObject to add the script to</param>
    public SavableMonoBehaviour addScript(SavableObjectInfo soi)
    {
        return (SavableMonoBehaviour)soi.gameObject.AddComponent(ScriptType);
    }

    public override bool Equals(object obj)
    {
        if (!obj.GetType().Equals(typeof(SavableObject))) { return false; }

        SavableObject so = (SavableObject)obj;
        if (so.scriptType != scriptType) { return false; }
        if (so.isSpawnedScript != isSpawnedScript)
        {
            //TODO: check to see if this is actually legal, it could be isSpawnedScript is actually variable
            throw new UnityException($"Two savable objects with same script type but different values! scriptType: {scriptType}, value: isSpawnedScript");
        }
        if (so.data.Keys.Count != data.Keys.Count)
        {
            throw new UnityException(
                $"Two savable objects with same script type but different values! scriptType: {scriptType}, value: data.Keys.Count ({data.Keys.Count},{so.data.Keys.Count})"
                );
        }

        //2025-02-16: copied from https://stackoverflow.com/a/141098/2336212
        foreach (KeyValuePair<short, object> kvp in data)
        {
            if (so.data[kvp.Key] != kvp.Value)
            {
                return false;
            }
        }

        //no differences found
        return true;
    }
}
