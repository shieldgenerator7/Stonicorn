
using UnityEngine;

public abstract class MemoryMonoBehaviour : MonoBehaviour
{
    private bool discovered;
    public bool Discovered
    {
        get => discovered;
        set
        {
            if (!discovered && value)
            {
                discovered = true;
                Managers.Object.saveMemory(this);
                nowDiscovered();
            }
        }
    }

    /// <summary>
    /// Called when this memory object is discovered for the first time
    /// </summary>
    protected abstract void nowDiscovered();

    /// <summary>
    /// Called when this memory object had been discovered before
    /// </summary>
    protected abstract void previouslyDiscovered();

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.isPlayerSolid())
        {
            Discovered = true;
        }
    }

    /// <summary>
    /// "Save": Returns the MemoryObject used to save this object's memory state
    /// </summary>
    /// <returns></returns>
    public MemoryObject getMemoryObject()
        => new MemoryObject(this, discovered);

    /// <summary>
    /// "Load": replaces its current memory state with the state in the given MemoryObject
    /// </summary>
    /// <param name="memObj"></param>
    public void acceptMemoryObject(MemoryObject memObj)
    {
        discovered = memObj.found;
        if (discovered)
        {
            previouslyDiscovered();
        }
    }
}
