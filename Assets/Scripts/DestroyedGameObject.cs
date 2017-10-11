using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is pasted onto an object that was broken apart
/// and set inactive
/// This script is a "tag" so that the GameManager can find it when collecting all objects
/// </summary>
public class DestroyedGameObject : SavableMonoBehaviour{

    public override bool isSpawnedScript()
    {
        return true;
    }

    public override SavableObject getSavableObject()
    {
        return new SavableObject(this);
    }
    public override void acceptSavableObject(SavableObject savObj)
    {
    }

}
