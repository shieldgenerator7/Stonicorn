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
    //TODO: find out how this keeps getting set to null
    public virtual string PrefabGUID => prefabAddress.AssetGUID;
    public int spawnStateId = -1;//-1 is an invalid Id but it forces save on new objects
    public int destroyStateId = int.MaxValue;//the game state id in which this object was destroyed (max value for not destroyed)

    [Header("Components to save")]
    [AutoInitialize(AllowUnfound =true), SerializeField, HideInInspector]
    private Rigidbody2D rb2d;
    public Rigidbody2D Rigidbody2D => rb2d;
    [AutoInitialize(AllowUnfound =true), SerializeField, HideInInspector]
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

    public SavableMonoBehaviour getSavableMonoBehaviour(Type type)
    {
        return savables.Where(smb=>smb.GetType() == type).FirstOrDefault();
    }

#if UNITY_EDITOR
    public virtual void autoset()
    {
        int changeCount = 0; 

        //Populate savable components
        if (!rb2d)
        {
            rb2d = GetComponent<Rigidbody2D>();
            if (rb2d)
            {
                Debug.LogWarning($"SavableObjectInfo setup: {gameObject.name}: Rigidbody2D set: {rb2d}", this);
                changeCount++;
            }
        }
        int prevCount = savables.Count;
        savables.Clear();
        savables = GetComponents<SavableMonoBehaviour>().ToList();
        if (savables.Count != prevCount)
        {
            Debug.LogWarning($"SavableObjectInfo setup: {gameObject.name}: savables list updated: {prevCount} -> {savables.Count}", this);
            changeCount++;
        }

        //setup
        changeCount += setup();

        //Set dirty
        if (changeCount > 0)
        {
        EditorUtility.SetDirty(this);
        }
    }

    public virtual int checkForErrors()
    {
        int errorCount = 0;

        //error: on an object that's not savable
        if (!rb2d && (savables == null || savables.Count == 0))
        {
            Debug.LogError($"SavableObjectInfo has no Rigidbody2D or any SavableMonoBehaviours! {gameObject.name}", this);
            errorCount++;
        }

        //error: no prefabGUID
        string[] exceptionList = new string[]
        {
            "_NPC",
        };
        if (string.IsNullOrEmpty(PrefabGUID) && !exceptionList.Any(ex=>gameObject.name.Contains(ex)))
        {
            Debug.LogError($"SavableObjectInfo has invalid PrefabGUID! prefabAddress: {prefabAddress}, PrefabGUID: {PrefabGUID}, go: {gameObject.name}", this);
            errorCount++;
        }

        return errorCount;
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
            //"id",
            "spawnStateId",
            "destroyStateId",
        };
        revertList.ForEach(revert =>
        {
            SerializedProperty property = so.FindProperty(revert);
            try
            {
                if (property.prefabOverride)
                {
            PrefabUtility.RevertPropertyOverride(property, InteractionMode.UserAction);

                    Debug.LogWarning($"SavableObjectInfo setup: {gameObject.name}: {revert} override reverted", this);
                    changeCount++;
                }
            }
            catch(ArgumentException ae)
            {
                Debug.LogError($"Trying to revert override on {property.name}, but failed. Moving on. error:  {ae.Message}", this);
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
                Debug.LogWarning($"SavableObjectInfo setup: {gameObject.name}: prefabAddress updated: {assetRef}", this);
            changeCount++;
        }
        }
        else
        {
            //Debug.LogError($"Cant find assetRef for {gameObject.name}: assetPath: {assetPath}, guid: {guid}, assetRef: {assetRef}, {assetRef?.AssetGUID}");
        }


        //
        return changeCount;
    }
#endif
}
