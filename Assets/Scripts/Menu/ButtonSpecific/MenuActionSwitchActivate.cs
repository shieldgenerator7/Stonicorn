using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionSwitchActivate : MenuActionSwitch
{
    public GameObject objectToActivate;

    public override void doAction(bool active)
    {
        objectToActivate.SetActive(active);
        MenuFrame mf = objectToActivate.GetComponent<MenuFrame>();
        if (active && mf)
        {
            mf.init();
            mf.frameCamera();
            MenuManager mm = FindObjectOfType<MenuManager>();
            if (!mm.frames.Contains(mf))
            {
                mm.frames.Add(mf);
            }
        }
    }

    public override bool getActiveState()
    {
        return objectToActivate.activeSelf;
    }
}
