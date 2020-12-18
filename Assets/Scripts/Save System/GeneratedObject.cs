using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratedObject : SavableMonoBehaviour
{
    [SerializeField]
    private string prefabName;

    private string spawnTag;

    public override bool IsSpawnedObject => true;

    public override string PrefabName => prefabName;

    public override string SpawnTag => spawnTag;

    public void init()
    {
        string tag = "" + System.DateTime.Now.Ticks;
        this.name += tag;
        spawnTag = tag;
    }

    public override SavableObject CurrentState
    {
        get => new SavableObject(this,
            "spawnTag", spawnTag
            );
        set => spawnTag = value.String("spawnTag");
    }
}
