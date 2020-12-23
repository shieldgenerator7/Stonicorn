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
    /// The unique identifier added to the game object's name,
    /// if this game object was spawned during runtime 
    /// </summary>
    /// <returns></returns>
    public virtual string SpawnTag => "";
}
