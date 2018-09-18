
using UnityEngine;

public abstract class MemoryMonoBehaviour : MonoBehaviour
{
    /// <summary>
    /// "Save": Returns the MemoryObject used to save this object's memory state
    /// </summary>
    /// <returns></returns>
    public abstract MemoryObject getMemoryObject();
    /// <summary>
    /// "Load": replaces its current memory state with the state in the given MemoryObject
    /// </summary>
    /// <param name="memObj"></param>
    public abstract void acceptMemoryObject(MemoryObject memObj);
}
