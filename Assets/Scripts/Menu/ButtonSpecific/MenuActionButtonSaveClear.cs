using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuActionButtonSaveClear : MenuActionButton
{
    public override void activate()
    {
        if (ES3.FileExists("merky.txt")){
            ES3.DeleteFile("merky.txt");
        }
        Managers.Game.resetGame(false);
    }
}
