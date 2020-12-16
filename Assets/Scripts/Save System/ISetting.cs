using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Setting object gets loaded when the game opens (or a save is loaded),
/// and gets saved when the game closes (or the save is unloaded).
/// It does not get rewound.
/// A Setting would usually also have a menu option to change its value.
/// Only one object of each type may save its settings with this interface
/// (unless you override the ID property to make the ID unique amogn members of the same class).
/// </summary>
public interface ISetting
{
    SettingScope Scope
    {
        get;
    }

    string ID
    {
        get;
    }

    SettingObject Setting
    {
        get;
        set;
    }

}
