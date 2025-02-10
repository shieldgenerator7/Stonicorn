using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

[DisallowMultipleComponent]
public class SavableObjectInfo : ObjectInfo, ISetupable
{
    [SerializeField]
    private AssetReference prefabAddress;
    public AssetReference PrefabAddress => prefabAddress;
    public virtual string PrefabGUID => prefabAddress.AssetGUID;
    public int spawnStateId = -1;//-1 is an invalid Id but it forces save on new objects
    public int destroyStateId = int.MaxValue;//the game state id in which this object was destroyed (max value for not destroyed)

    [Header("Components to save")]
    [AutoInitialize(AllowUnfound =true), SerializeField, HideInInspector]
    private Rigidbody2D rb2d;
    public Rigidbody2D Rigidbody2D => rb2d;
    //[AutoInitialize, SerializeField, HideInInspector]
    public List<SavableMonoBehaviour> savables;

    public SavableObjectInfoData Data
    {
        get => new SavableObjectInfoData(this);
        set
        {
            SavableObjectInfoData soid = value;
            this.Id = soid.id;
            this.spawnStateId = soid.spawnStateId;
            this.destroyStateId = soid.destroyStateId;
        }
    }

#if UNITY_EDITOR
    public virtual void autoset()
    {
        if (setup() > 0)
        {
        //Set dirty
        EditorUtility.SetDirty(this);
        }
    }

    public virtual int setup()
    {
        //dont process this in scene objects
        if (gameObject.scene.buildIndex > 0)
        {
            return 0;
        }

        //we're in a prefab object now, so all good to process
        int changeCount = 0;

        //revert unneeded overrides
        SerializedObject so = new SerializedObject(this);
        List<string> revertList = new List<string>()
        {
            "id",
            "spawnStateId",
            "destroyStateId",
        };
        revertList.ForEach(revert =>
        {
            SerializedProperty property = so.FindProperty(revert);
            try
            {
                Debug.Log($"SavableObjectInfo setup: {revert} overridden? {property.prefabOverride}");
                if (property.prefabOverride)
                {
            PrefabUtility.RevertPropertyOverride(property, InteractionMode.UserAction);
                    changeCount++;
                }
            }
            catch(ArgumentException ae)
            {
                Debug.LogError($"Trying to revert override on {property.name}, but failed. Moving on. error:  {ae.Message}");
            }
        });

        //set Prefab Address
        string assetPath = AssetDatabase.GetAssetPath(gameObject);
        string guid = "";
        if (assetPath == null || assetPath.Trim() == "")
        {
            List<string> guids = AssetDatabase.FindAssets(gameObject.name).ToList();
            guid = guids.Find(guid =>
            {
                string[] split = AssetDatabase.GUIDToAssetPath(guid).Split('/', '.');
                return gameObject.name == split[split.Length - 2];
            });
        }
        else
        {
            guid = AssetDatabase.AssetPathToGUID(
                AssetDatabase.GetAssetPath(gameObject)
            );
        }
        //TODO: investigate why unity auto-setting this isnt working anymore
        AssetReference assetRef = new AssetReference(guid);
        if (assetRef != null && assetRef.IsValid()){//!string.IsNullOrEmpty(assetRef.AssetGUID)) {
        if (prefabAddress != assetRef)
        {
            prefabAddress = assetRef;
            changeCount++;
        }
        }
        else
        {
            //Debug.LogError($"Cant find assetRef for {gameObject.name}: assetPath: {assetPath}, guid: {guid}, assetRef: {assetRef}, {assetRef?.AssetGUID}");
        }

        //Populate savable components
        if (!rb2d)
        {
        rb2d = GetComponent<Rigidbody2D>();
            if (rb2d)
            {
            changeCount++;
            }
        }
        int prevCount = savables.Count;
        savables.Clear();
        savables = GetComponents<SavableMonoBehaviour>().ToList();
        if (savables.Count != prevCount)
        {
            changeCount++;
        }

        //
        return changeCount;
    }
#endif
}
