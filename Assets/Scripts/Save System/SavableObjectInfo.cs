using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SavableObjectInfo : ObjectInfo
{
    [SerializeField]
    private AssetReference prefabAddress;
    public AssetReference PrefabAddress => prefabAddress;
    public virtual string PrefabGUID => prefabAddress.AssetGUID;
    public int spawnStateId = -1;//-1 is an invalid Id but it forces save on new objects
    public int destroyStateId = int.MaxValue;//the game state id in which this object was destroyed (max value for not destroyed)

    [Header("Components to save")]
    public Rigidbody2D rb2d;
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
            PrefabUtility.RevertPropertyOverride(property, InteractionMode.UserAction);
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
        prefabAddress = new AssetReference(guid);

        //Populate savable components
        rb2d = GetComponent<Rigidbody2D>();
        savables = GetComponents<SavableMonoBehaviour>().ToList();

        //Set dirty
        EditorUtility.SetDirty(this);
    }
#endif
}
