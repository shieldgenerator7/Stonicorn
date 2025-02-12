using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSwitchActivate : MenuActionSwitch
{
    public GameObject objectToActivate;

    [AutoInitialize(Container = "objectToActivate", AllowUnfound =true),SerializeField,HideInInspector]
    private MenuFrame mf;

    public override void doAction(bool active)
    {
        objectToActivate.SetActive(active);
        if (active && mf)
        {
            mf.frameCamera();
            MenuManager mm = Managers.Menu;
            mm.AddFrame(mf);
        }
    }

    public override bool getActiveState()
    {
        return objectToActivate.activeSelf;
    }
}
