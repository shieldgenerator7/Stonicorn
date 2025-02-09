using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionButtonExit : MenuActionButton
{
    public override void activate()
    {
        Debug.Log("Quitting...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
