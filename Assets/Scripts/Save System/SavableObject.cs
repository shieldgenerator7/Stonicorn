
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
    private Dictionary<string, System.Object> data = new Dictionary<string, System.Object>();
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
            data.Add((string)pairs[i], pairs[i + 1]);
        }
        return this;
    }

    public System.Object get(string name)
        => data[name];
    public bool Bool(string name)
        => (bool)data[name];
    public int Int(string name)
        => (int)data[name];
    public float Float(string name)
        => (float)data[name];
    public string String(string name)
        => (string)data[name];
    public Vector2 Vector2(string name)
        => (Vector2)data[name];


    public SavableObject addList<T>(string key, List<T> list)
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

    public SavableObject addDictionary<K, V>(string key, Dictionary<K, V> dict)
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
    public virtual Component addScript(GameObject go)
    {
        return go.AddComponent(ScriptType);
    }
}
