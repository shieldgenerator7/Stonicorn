
using System.Collections.Generic;
using UnityEngine;

public class SavableObject
{

    //this class stores variables that need to be saved from MonoBehaviours

    public Dictionary<string, System.Object> data = new Dictionary<string, System.Object>();
    /// <summary>
    /// True if it's an object that spawned during runtime
    /// </summary>
    public bool isSpawnedObject;//whether this SO's game object was spawned during run time
    public bool isSpawnedScript;//whether this SO's script was attached to its game object during run time
    public string scriptType;//the type of script that saved this SavableObject
    public string prefabName="";//if isSpawnedObject, what the prefab name is. Prefab must be in the Resources folder
    
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
        if (pairs.Length % 2 != 0)
        {
            throw new UnityException("Pairs has an odd amount of parameters! pairs.Length: "+pairs.Length);
        }
        for (int i = 0; i < pairs.Length; i += 2)
        {
            data.Add((string)pairs[i], pairs[i + 1]);
        }
        if (smb.isSpawnedObject())
        {
            isSpawnedObject = true;
            prefabName = smb.getPrefabName();
        }
        if (smb.isSpawnedScript())
        {
            isSpawnedScript = true;
        }
    }

    /// <summary>
    /// Spawn this saved object's game object
    /// This method is used during load
    /// precondition: the game object does not already exist (or at least has not been found)
    /// </summary>
    /// <returns></returns>
    public GameObject spawnObject()
    {
        GameObject prefab = (GameObject)GameObject.Instantiate(Resources.Load(prefabName));
        return (GameObject)prefab;
    }

    public System.Type getSavableMonobehaviourType()
    {
        return getSavableMonobehaviourType(scriptType);
    }
    /// <summary>
    /// The definitive list of known savable types
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static System.Type getSavableMonobehaviourType(string typeName)
    {
        switch (typeName)
        {
            case "CrackedGroundChecker":
                return typeof(CrackedGroundChecker);
            case "GestureManager":
                return typeof(GestureManager);
            case "Rigidbody2DLock":
                return typeof(Rigidbody2DLock);
            case "ShieldBubbleController":
                return typeof(ShieldBubbleController);
            case "PowerConduit":
                return typeof(PowerConduit);
            case "ExplosionOrbController":
                return typeof(ExplosionOrbController);
            case "HardMaterial":
                return typeof(HardMaterial);
            case "CrackedPiece":
                return typeof(CrackedPiece);
            case "NPCController":
                return typeof(NPCController);
            case "NPCVoiceLine":
                return typeof(NPCVoiceLine);
            default:
                throw new KeyNotFoundException("The type name \"" + typeName + "\" was not found. It might not be a SavableMonoBehaviour or might not exist.");
        }
    }

    ///<summary>
    ///Adds this SavableObject's SavableMonobehaviour to the given GameObject
    ///</summary>
    ///<param name="go">The GameObject to add the script to</param>
    public virtual Component addScript(GameObject go)
    {
        return go.AddComponent(getSavableMonobehaviourType());
    }
}
