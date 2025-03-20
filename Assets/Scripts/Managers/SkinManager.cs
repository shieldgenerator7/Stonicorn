using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SkinManager : MonoBehaviour, ISetting
{
    [SerializeField]
    private List<string> foundSkinList = new List<string>();
    private int _skinIndex = -1;

    [SerializeField]
    private List<GameObject> skinLibrary = new List<GameObject>();

    public Skin Skin
    {
        get => (_skinIndex >= 0) ? getSkin(foundSkinList[_skinIndex]) : null;
        set
        {
            addSkin(value);
            int index = foundSkinList.IndexOf(value.name);
            if (index != -1)
            {
                SkinIndex = index;
            }
        }
    }
    public int SkinIndex
    {
        get => _skinIndex;
        set
        {
            _skinIndex = Mathf.Clamp(value, 0, foundSkinList.Count - 1);
            onSkinChanged?.Invoke(getSkin(value));
        }
    }
    public event Action<Skin> onSkinChanged;

    public int FoundSkinCount => foundSkinList.Count;

    public void addSkin(Skin skin)
    {
        if (foundSkinList.Contains(skin.name)) { return; }

        foundSkinList.Add(skin.name);
        _skinIndex = foundSkinList.IndexOf(skin.name);
    }

    public Skin getSkin(int index)
    {
        if (index < 0 || index >= foundSkinList.Count)
        {
            return null;
        }
        return getSkin(foundSkinList[index]);
    }
    public Skin getSkin(string name)
    {
        return skinLibrary
            .ConvertAll(go=>go.GetComponent<Skin>())
            .Find(skin=>skin.name == name);
    }

    public SettingScope Scope => SettingScope.SAVE_FILE;

    public string ID => "SkinManager";

    public SettingObject Setting { 
        get => new SettingObject(ID)
            .addList("foundSkinList", foundSkinList);
        set {
            foundSkinList = value.List<string>("foundSkinList");
        }
    }


#if UNITY_EDITOR
    [Initializer]
    private List<GameObject> init_skinLibrary()
        => FindObjectsByType<Skin>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)
            .ToList()
            .ConvertAll(skin => PrefabUtility.GetCorrespondingObjectFromSource(skin.gameObject))
            .Distinct().ToList();
#endif
}
