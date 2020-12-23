using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrackedPiece : SavableMonoBehaviour
{
    //2017-05-02: the script to make broken pieces work with time rewind system
    //Apply to parent object of broken object prefab

    public string prefabName;
    public string spawnTag;//the tag to make it unique among the other pieces

    public override bool IsSpawnedScript => true;

    public override string SpawnTag => spawnTag;

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "prefabName", prefabName,
            "spawnTag", spawnTag
            );
        set
        {
            prefabName = value.String("prefabName");
            spawnTag = value.String("spawnTag");
        }
    }
}
