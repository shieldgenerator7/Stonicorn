
using UnityEngine;
using UnityEngine.SceneManagement;

public class MemoryObject
{
    //this class stores variables of MonoBehaviours that have memories to be saved

    public bool found = false;//whether this memory has been obtained by Merky yet
   
    public MemoryObject() { }

    public MemoryObject(MemoryMonoBehaviour mmb, bool foundYet)
    {
        this.found = foundYet;
    }
}
