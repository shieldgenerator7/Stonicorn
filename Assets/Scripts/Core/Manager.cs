using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Manager : MonoBehaviour, ISetting
{
    protected GameData data;

    public void init(GameData data)
    {
        this.data = data;
    }

    public virtual SettingScope Scope => SettingScope.SAVE_FILE;

    public virtual string ID => "Manager";

    public virtual SettingObject Setting
    {
        get => null;
        set { }
    }
}
