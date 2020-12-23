
using System.Collections.Generic;
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

    /// <summary>
    /// Spawn this saved object's game object
    /// This method is used during load
    /// precondition: the game object does not already exist (or at least has not been found)
    /// </summary>
    /// <returns></returns>
    public GameObject spawnObject(string goName, string prefabName)
    {
        GameObject newGO = (GameObject)GameObject.Instantiate(
            Resources.Load("Prefabs/" + prefabName)
            );
        newGO.name = goName;
        return newGO;
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
