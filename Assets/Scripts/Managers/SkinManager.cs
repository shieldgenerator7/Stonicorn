using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SkinManager : MonoBehaviour, ISetting
{
    [SerializeField]
    private List<Skin> foundSkinList = new List<Skin>();
    private int _skinIndex = -1;

    [SerializeField]
    private List<GameObject> skinLibrary = new List<GameObject>();

    public Skin Skin
    {
        get => (_skinIndex >= 0) ? foundSkinList[_skinIndex] : null;
        set
        {
            if (!foundSkinList.Contains(value))
            {
                foundSkinList.Add(value);
            }
            int index = foundSkinList.IndexOf(value);
            if (index != -1)
            {
                _skinIndex = index;
            }
        }
    }
    public int SkinIndex
    {
        get => _skinIndex;
        set => _skinIndex = Mathf.Clamp(value, 0, foundSkinList.Count - 1);
    }

    public int FoundSkinCount => foundSkinList.Count;

    public void addSkin(Skin skin)
    {
        if (foundSkinList.Contains(skin)) { return; }

        foundSkinList.Add(skin);
    }

    public Skin getSkin(int index)
    {
        return foundSkinList[index];
    }

    public SettingScope Scope => SettingScope.SAVE_FILE;

    public string ID => "SkinManager";

    public SettingObject Setting { 
        get => new SettingObject(ID)
            .addList("foundSkinList", foundSkinList);
        set {
            foundSkinList = value.List<Skin>("foundSkinList");
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
