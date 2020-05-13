using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionButtonURL : MenuActionButton
{//2019-01-15: copied from MilestoneActivatorURL
    public string url;//the URL to go to

    public override void activate()
    {
        Application.OpenURL(url);
    }
}
