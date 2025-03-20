using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour, ISetting
{
    private List<Skin> foundSkinList = new List<Skin>();
    private int _skinIndex = -1;

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
}
