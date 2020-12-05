using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SavableMonoBehaviour : MonoBehaviour
{
    /// <summary>
    /// The SavableObject that contains this object's configuration state
    /// </summary>
    public abstract SavableObject CurrentState { get; set; }

    //
    //Spawned Objects and Scripts
    //

    /// <summary>
    /// True if this script was spawned during runtime
    /// </summary>
    /// <returns></returns>
    public virtual bool IsSpawnedScript => false;

    /// <summary>
    /// True if this script's game object was spawned during runtime
    /// </summary>
    /// <returns></returns>
    public virtual bool IsSpawnedObject => false;

    /// <summary>
    /// The unique identifier added to the game object's name,
    /// if this game object was spawned during runtime 
    /// </summary>
    /// <returns></returns>
    public virtual string SpawnTag => "";

    /// <summary>
    /// Returns the name of the prefab for this script
    /// </summary>
    /// <returns></returns>
    public virtual string PrefabName
        => throw new System.MissingMethodException(
            "This method is not supported for class "
            + GetType().Name 
            + " because it is not a spawned object."
            );
}
