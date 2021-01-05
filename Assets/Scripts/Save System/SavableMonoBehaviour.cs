using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SavableMonoBehaviour : MonoBehaviour
{
    public abstract void init();

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
    /// The priority for loading this SMB's state,
    /// higher priority gets loaded first
    /// </summary>
    public virtual int Priority => 0;
}
