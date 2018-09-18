using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SavableMonoBehaviour: MonoBehaviour {
    /// <summary>
    /// "Save": Returns the SavableObject used to save this object's configuration state
    /// </summary>
    /// <returns></returns>
    public abstract SavableObject getSavableObject();
    /// <summary>
    /// "Load": replaces its current state with the state in the given SavableObject
    /// </summary>
    /// <param name="savObj"></param>
    public abstract void acceptSavableObject(SavableObject savObj);

    //
    //Spawned Objects and Scripts
    //

    /// <summary>
    /// True if this script was spawned during runtime
    /// </summary>
    /// <returns></returns>
    public virtual bool isSpawnedScript()
    {
        return false;
    }

    /// <summary>
    /// True if this script's game object was spawned during runtime
    /// </summary>
    /// <returns></returns>
    public virtual bool isSpawnedObject()
    {
        return false;
    }
    /// <summary>
    /// The unique identifier added to the game object's name,
    /// if this game object was spawned during runtime 
    /// </summary>
    /// <returns></returns>
    public virtual string getSpawnTag()
    {
        return "";
    }
    /// <summary>
    /// Returns the name of the prefab for this script
    /// </summary>
    /// <returns></returns>
    public virtual string getPrefabName()
    {
        throw new System.MissingMethodException("This method is not supported for class " + GetType().Name + " because it is not a spawned object.");
    }
}
