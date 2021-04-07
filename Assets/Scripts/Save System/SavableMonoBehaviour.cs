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

    /// <summary>
    /// True if this script was spawned during runtime
    /// </summary>
    /// <returns></returns>
    public virtual bool IsSpawnedScript => false;
}
